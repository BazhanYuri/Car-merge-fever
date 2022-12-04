using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HomaGames.HomaBelly
{
    public class EventDispatcher : IDisposable
    {
        private const int HEARTBEAT_MILLISECONDS = 1000;
        private const int CONCURRENT_REQUESTS = 5;
        private const int REQUEST_TIMEOUT_SECONDS = 10;
        private const int MAX_RETRIES_PER_EVENT = 3;

        private HomaAnalyticsOptions m_analyticsOptions = null;
        
        /// <summary>
        /// Pending events to be sent to the server
        /// </summary>
        private ConcurrentQueue<PendingEvent> m_pendingEvents = new ConcurrentQueue<PendingEvent>();

        /// <summary>
        /// We will keep events in this list until we receive a response. We will retry 
        /// </summary>
        private readonly List<PendingEvent> m_sentEvents = new List<PendingEvent>();

        private readonly object m_sentEventsLock = new object();

        private HttpClient m_httpClient = null;
        private CancellationTokenSource m_retryHeartbeatCancellationToken;
        private bool m_toggled = true;

        public int PendingEventsCount => m_pendingEvents.Count;
        // ReSharper disable once InconsistentlySynchronizedField
        public int WaitingEventsResponseCount => m_sentEvents.Count;

        public void Initialize(HomaAnalyticsOptions options)
        {
            m_analyticsOptions = options;

            InitializeHttpClient(false);
            
            m_httpClient.Timeout = new TimeSpan(0, 0, 0, REQUEST_TIMEOUT_SECONDS);

            RetrySentEventsHeartbeat();
        }

        private void InitializeHttpClient(bool useCharlesProxy)
        {
            if (Application.isEditor && useCharlesProxy)
            {
                var httpClientHandler = new HttpClientHandler()
                {
                    Proxy = new System.Net.WebProxy("http://localhost:8888", false),
                    UseProxy = true
                };

                m_httpClient = new HttpClient(httpClientHandler);
            }
            else
            {
                m_httpClient = new HttpClient();
            }
        }

        /// <summary>
        /// Send pending event to the server.
        /// </summary>
        public void DispatchPendingEvent(PendingEvent pendingEvent)
        {
            if(!m_toggled)
            {
                // Dispatching events is disabled, the event will be discarded
                pendingEvent.Dispatched();
                return;
            }
            
            m_pendingEvents.Enqueue(pendingEvent);
            
            TryToSendPendingEvents();
        }

        private void TryToSendPendingEvents()
        {
            int eventsToSend = m_pendingEvents.Count;
            
            int availableRequests;
            lock (m_sentEventsLock)
            {
                availableRequests = CONCURRENT_REQUESTS - m_sentEvents.Count;
            }
            
            if (eventsToSend > 0 && availableRequests > 0)
            {
                for (int i = 0; i < availableRequests; i++)
                {
                    if (m_pendingEvents.TryDequeue(out var eventToSend))
                    {
                        PostEvent(eventToSend);
                    }
                }
            }
        }
        
        private async void PostEvent(PendingEvent pendingEvent)
        {
            lock (m_sentEventsLock)
            {
                m_sentEvents.Add(pendingEvent);
            }
            
            try
            {
                var cancellationToken = pendingEvent.PrepareToSend(REQUEST_TIMEOUT_SECONDS);
                
                var data = new StringContent(pendingEvent.Json);
                // Removing the default header
                data.Headers.Remove("Content-Type");
                // Adding our custom header without any validation
                data.Headers.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

                pendingEvent.PostRetries++;
                
                Task<HttpResponseMessage> postTask = null;
                if (pendingEvent.SkipStorageAndSyncDispatch)
                {
                    postTask = Task.Run(() => m_httpClient.PostAsync(m_analyticsOptions.EndPointUrl, data,
                        cancellationToken), cancellationToken);
                    
                    postTask.Wait(cancellationToken);
                }
                else
                {
                    postTask = m_httpClient.PostAsync(m_analyticsOptions.EndPointUrl, data, cancellationToken);
                    await postTask;
                }
                 
                var response = postTask.Result;

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    if (m_analyticsOptions.VerboseLogs)
                    {
                        Debug.Log($"[HomaAnalytics Event] Event {pendingEvent.EventName}-{pendingEvent.Id} sent successfully.");
                    }

                    pendingEvent.Dispatched();
                    RemoveSentEvent(pendingEvent, false);
                    TryToSendPendingEvents();
                }
                else
                {
                    var resultString = await response.Content.ReadAsStringAsync();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    
                    if (!string.IsNullOrEmpty(resultString))
                    {
                        // Read error details
                        if (Json.Deserialize(resultString) is Dictionary<string, object> dictionary)
                        {
                            // Detect any error
                            if (dictionary.ContainsKey("status") && dictionary.ContainsKey("message"))
                            {
                                Debug.LogError(
                                    $"[HomaAnalytics] Response: {response.StatusCode} {Convert.ToString(dictionary["message"])} Event: {pendingEvent}");
                            }
                        }
                        else
                        {
                            Debug.LogError($"[HomaAnalytics Event] Response:{response.StatusCode} {response.ReasonPhrase} {response.RequestMessage} Event: {pendingEvent}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[HomaAnalytics Event] {(int) response.StatusCode}: {response.ReasonPhrase} Event: {pendingEvent}");
                    }
                    
                    if (pendingEvent.PostRetries >= MAX_RETRIES_PER_EVENT)
                    {
                        RemoveSentEvent(pendingEvent, true);
                        pendingEvent.Dispatched();
                        if (m_analyticsOptions.VerboseLogs)
                        {
                            Debug.LogWarning($"[HomaAnalytics Event] Event {pendingEvent.EventName}:{pendingEvent.Id} was not sent. Max retries reached.");
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Do nothing. This is expected because our retry policy will cancel ongoing events
                // and we will try to send them again.
            }
            catch (HttpRequestException e)
            {
                // Do nothing. This happens where there isn't connection.
                // wait for next try.
                HomaGamesLog.Warning($"[HomaAnalytics Event] Can't post message Event: {pendingEvent} {e}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[HomaAnalytics Event] Can't post message Event: {pendingEvent} {e}");
                RemoveSentEvent(pendingEvent,true);
                pendingEvent.Dispatched();
            }
        }
        
        /// <summary>
        /// We will check at some intervals if some events have to be sent again.
        /// </summary>
        private async void RetrySentEventsHeartbeat()
        {
            while (true)
            {
                if (!Application.isPlaying)
                {
                    return;
                }
                    
                await Task.Delay(HEARTBEAT_MILLISECONDS);

                if (!Application.isPlaying)
                {
                    return;
                }

                var eventsToRetry = CancelAllSentEvents(sentEvent => sentEvent.GetElapsedTime() > REQUEST_TIMEOUT_SECONDS);
                    
                foreach (var pendingEvent in eventsToRetry)
                {
                    HomaGamesLog.Warning($"[HomaAnalytics] Event: {pendingEvent.EventName}:{pendingEvent.Id} timeout. It will be sent again.");
                    PostEvent(pendingEvent);
                }
            }
        }

        private void RemoveSentEvent(PendingEvent sentEvent,bool cancel)
        {
            lock (m_sentEventsLock)
            {
                m_sentEvents.Remove(sentEvent);
            }

            if (cancel)
            {
                sentEvent.CancelAndDispose();
            }
            else
            {
                sentEvent.Dispose();
            }
        }


        /// <summary>
        /// Clear pending events and cancel active requests
        /// </summary>
        private void CancelAllEvents(bool removeEventsFromDisk)
        {
            // Pending events aren't sent yet, so we only need to remove and notify about it
            while (m_pendingEvents.TryDequeue(out var pendingEvent))
            {
                pendingEvent.Dispose();
                if (removeEventsFromDisk)
                {
                    pendingEvent.Dispatched();    
                }
            }

            var sentEvents = CancelAllSentEvents(pendingEvent => true);

            if (removeEventsFromDisk)
            {
                foreach (var sentEvent in sentEvents)
                {
                    sentEvent.Dispatched();
                }
            }
        }

        /// <summary>
        /// Cancel all sent events that meets the given condition.
        /// </summary>
        private List<PendingEvent> CancelAllSentEvents(Func<PendingEvent,bool> conditionToCancel)
        {
            var cancelledEvents = new List<PendingEvent>();
            try
            {
                lock (m_sentEventsLock)
                {
                    for (var index = m_sentEvents.Count - 1; index >= 0; index--)
                    {
                        var sentEvent = m_sentEvents[index];
                        if (conditionToCancel(sentEvent))
                        {
                            RemoveSentEvent(sentEvent, true);
                            cancelledEvents.Add(sentEvent);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[HomaAnalytics Event] Error while canceling events {e}");
            }

            return cancelledEvents;
        }

        /// <summary>
        /// If false, we will stop dispatching events to the server
        /// </summary>
        public void Toggle(bool toggle)
        {
            m_toggled = toggle;

            if (!m_toggled)
            {
                CancelAllEvents(true);
            }
        }

        public void Dispose()
        {
            CancelAllEvents(false);
            
            // Avoid leaving the heartbeat background process alive 
            if (m_retryHeartbeatCancellationToken != null 
                && !m_retryHeartbeatCancellationToken.IsCancellationRequested)
            {
                m_retryHeartbeatCancellationToken.Cancel();
                m_retryHeartbeatCancellationToken.Dispose();
                m_retryHeartbeatCancellationToken = null;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class HomaAnalytics : IDisposable
    {
        public const int MAX_EVENT_NAME_LENGTH = 64;
        public const int MAX_EVENT_VALUES_LENGTH = 2048;
        private const int SLOT_MAX_EVENTS = 10;
        private const int SLOT_TIME_TO_AUTO_DISPATCH_MS = 500;
        private const int MAX_SIZE_IN_BYTES = 1000000; // 1MB
        private const int CRITICAL_FPS_THRESHOLD = 15;
        private const int SECONDS_BEFORE_START_MEASURING_FPS = 30;
        
        public static readonly string SLOTS_PATH = $"{Application.persistentDataPath}/EventStorage";
        

        
        #region Player Pref Keys
        private const string INSTALL_ID_KEY = "ha_install_id";
        private const string SESSION_OPENED_TIME_STAMP_KEY = "ha_session_opened_time_stamp";
        private const string SESSION_ID_KEY = "ha_session_id";
        #endregion
        
        #region Event Names
        private const string SESSION_OPENED_EVENT_NAME = "session_opened";
        private const string APPLICATION_AWAKEN_EVENT_NAME = "application_awaken";
        private const string APPLICATION_PAUSED_EVENT_NAME = "application_paused";
        private const string APPLICATION_FOCUSED_EVENT_NAME = "application_focused";
        private const string APPLICATION_QUIT_EVENT_NAME = "application_quit";
        private const string NEW_INSTALL_EVENT_NAME = "new_install";
        #endregion
        
        #region Event Categories
        public const string PROGRESSION_CATEGORY = "progression_event";
        public const string REVENUE_CATEGORY = "revenue_event";
        public const string DESIGN_CATEGORY = "design_event";
        public const string AD_CATEGORY = "ad_event";
        public const string SKELETON_CATEGORY = "skeleton_event";
        public const string PROFILE_CATEGORY = "profile_event";
        
        /// <summary>
        ///  Used for system events like game opened, session opened/closed, etc.
        /// </summary>
        public const string SYSTEM_CATEGORY = "system_event";
        #endregion


        private static readonly Dictionary<string, bool> VALID_CATEGORIES = new Dictionary<string, bool>()
        {
            {PROGRESSION_CATEGORY,true},
            {REVENUE_CATEGORY,true},
            {DESIGN_CATEGORY,true},
            {AD_CATEGORY,true},
            {SKELETON_CATEGORY,true},
            {SYSTEM_CATEGORY,true},
            {PROFILE_CATEGORY,true},
        };

        private HomaAnalyticsOptions m_analyticsOptions = null;
        private readonly Queue<RuntimeAnalyticsEvent> m_preInitializationEvents = new Queue<RuntimeAnalyticsEvent>();
        private readonly StringBuilder m_stringBuilderForDebugInfo = new StringBuilder();
        private bool m_analyticsEnabled = true;
        private bool m_initialized = false;
        private bool m_firstSessionOpened = false;
        private string m_installId = null;
        private string m_sessionId = null;
        private DateTime m_sessionOpenedTimeStampInSeconds = DateTime.MinValue;
        private EventDispatcher m_eventDispatcher = new EventDispatcher();
        private EventSlotManager m_eventSlotManager = null;
        private string m_persistentDataPath;

        /// <summary>
        /// Get events saved into disk
        /// </summary>
        public int TotalSavedEvents => m_eventSlotManager?.GetTotalSavedEvents() ?? 0;
        
        /// <summary>
        /// Get total events dispatched
        /// </summary>
        public int DispatchedEvents => m_eventSlotManager?.GetTotalDispatchedEvents() ?? 0;
        
        /// <summary>
        /// Count events that are waiting to be sent when HomaAnalytics is initialized.
        /// </summary>
        public int PreInitializationEventsCount => m_preInitializationEvents.Count;
        
        /// <summary>
        /// Events waiting to be sent.
        /// </summary>
        public int PendingEventsCount => m_eventDispatcher.PendingEventsCount;
        
        /// <summary>
        /// Events which response we are waiting to receive from the server. 
        /// </summary>
        public int WaitingEventsResponseCount => m_eventDispatcher.WaitingEventsResponseCount;
        
        public bool Initialized => m_initialized;

        /// <summary>
        /// Although you can start to track events in HomaAnalytics after the objects is constructed,
        /// we won't send any events until Initialize() is called.
        /// </summary>
        public HomaAnalytics(HomaAnalyticsOptions homaAnalyticsOptions)
        {
            m_analyticsOptions = homaAnalyticsOptions;
            m_persistentDataPath = Application.persistentDataPath;
        }

        /// <summary>
        /// You can override the options used in the constructor. Useful if you have some delayed initialization.
        /// </summary>
        public void Initialize(HomaAnalyticsOptions overrideOptionsUsedInConstructor = null)
        {
            if (m_initialized)
            {
                Debug.LogWarning("[HomaAnalytics] Analytics already initialized.");
                return;
            }
                
            if (overrideOptionsUsedInConstructor != null)
            {
                m_analyticsOptions = overrideOptionsUsedInConstructor;
            }

            m_eventDispatcher.Initialize(m_analyticsOptions);
            
            m_eventSlotManager = new EventSlotManager(m_eventDispatcher,
                SLOTS_PATH,
                SLOT_MAX_EVENTS,
                SLOT_TIME_TO_AUTO_DISPATCH_MS,
                MAX_SIZE_IN_BYTES);

            // Don't toggle analytics. Maybe a developer want to disable them before initializing the service 
            // m_analyticsEnabled = true;
            
            RuntimeAnalyticsEvent.UserId = Identifiers.HomaGamesId;

            CheckFirstSessionOpened();

            if (m_analyticsOptions.SendFpsEvents)
            {
                HomaAnalyticsFpsTool.CreateFpsTool(this,SECONDS_BEFORE_START_MEASURING_FPS,CRITICAL_FPS_THRESHOLD);
            }
            
            var proxyGameObject = new GameObject("HomaAnalyticsProxy");
            proxyGameObject.SetActive(false);
            proxyGameObject.hideFlags = HideFlags.HideAndDontSave;
            var componentProxy = proxyGameObject.AddComponent<HomaAnalyticsComponentProxy>();
            componentProxy.HomaAnalyticsInstance = this;
            // Activate proxy GO after setting WeakReference to gather first events
            proxyGameObject.SetActive(true);

            HomaAnalyticsLogListener.SetLogReceivedCallback(OnLogReceived);
            
            // Important: Initialize before tracking pre initialization events
            m_initialized = true;
                
            while (m_preInitializationEvents.Count > 0)
            {
                TrackEvent(m_preInitializationEvents.Dequeue());
            }
                
            if (m_analyticsOptions.VerboseLogs)
            {
                Debug.Log($"[HomaAnalytics] Initialized. Pending Events: {m_preInitializationEvents.Count}");
                Debug.Log($"[HomaAnalytics] Options: {m_analyticsOptions}");
            }
        }

        private void OnLogReceived(string condition, string stacktrace, LogType type)
        {
            if (! m_analyticsOptions.RecordLogs)
                return;
            
            if (type == LogType.Log
                || condition.StartsWith("[HomaAnalytics Event]"))
                return;

            TrackEvent(new LogAnalyticsEvent(condition, stacktrace, type));
        }
        
        /// <summary>
        /// Return true if the installation is new.
        /// </summary>
        private bool RecoverOrGenerateInstallId()
        {
            m_installId = PlayerPrefs.GetString(INSTALL_ID_KEY, null);

            bool newInstall = false;
            if (string.IsNullOrWhiteSpace(m_installId))
            {
                m_installId = Guid.NewGuid().ToString();
                newInstall = true;
                if (m_analyticsOptions.VerboseLogs)
                {
                    Debug.Log($"[HomaAnalytics] New install id generated: {m_installId}");
                }

                PlayerPrefs.SetString(INSTALL_ID_KEY, m_installId);
                PlayerPrefs.Save();
            }

            return newInstall;
        }

        public void ToggleAnalytics(bool toggle)
        {
            if (m_analyticsOptions.VerboseLogs && m_analyticsEnabled != toggle)
            {
                Debug.Log($"[HomaAnalytics] Toggled: {toggle}");
            }
            
            m_analyticsEnabled = toggle;
            
            if (!toggle)
            {
                m_preInitializationEvents.Clear();
            }
            
            m_eventDispatcher.Toggle(toggle);
        }

        /// <summary>
        /// Check if first session has been opened.
        /// We shouldn't have events until the first open session event is sent.
        /// </summary>
        private void CheckFirstSessionOpened()
        {
            if (m_firstSessionOpened)
            {
                return;
            }
            
            // Order is important to avoid StackOverflowException
            m_firstSessionOpened = true;
            
            bool newInstall = RecoverOrGenerateInstallId();
            CheckNewSession();

            // Send the new install after the session is opened
            if (newInstall)
            {
                var newInstallEvent = new RuntimeAnalyticsEvent(NEW_INSTALL_EVENT_NAME, SYSTEM_CATEGORY);
                TrackEvent(newInstallEvent);
            }
        }

        /// <summary>
        /// We only generate a new session id and we send the open session event
        /// if the time threshold with the application closed is overcome.
        /// Calling OpenSession always will update the session time stamp
        /// </summary>
        private void CheckNewSession()
        {
            // First time we open a session, check if there is one saved in player prefs
            if (string.IsNullOrEmpty(m_sessionId))
            {
                string sessionOpenedTimeStampString = PlayerPrefs.GetString(SESSION_OPENED_TIME_STAMP_KEY, null);
                if (!string.IsNullOrEmpty(sessionOpenedTimeStampString))
                {
                    DateTime.TryParse(sessionOpenedTimeStampString, out m_sessionOpenedTimeStampInSeconds);
                }

                m_sessionId = PlayerPrefs.GetString(SESSION_ID_KEY, null);
            }
            
            // Check if we have to generate a new session id
            var elapsedSeconds = Mathf.Abs((float)(DateTime.UtcNow - m_sessionOpenedTimeStampInSeconds).TotalSeconds);
            if(string.IsNullOrWhiteSpace(m_sessionId) 
               || elapsedSeconds > m_analyticsOptions.SecondsToGenerateNewSessionId)
            {
                m_sessionId = Guid.NewGuid().ToString();
                PlayerPrefs.SetString(SESSION_ID_KEY,m_sessionId);
                
                if (m_analyticsOptions.VerboseLogs)
                {
                    Debug.Log($"[HomaAnalytics] New session id set: {m_sessionId}");
                }
                
                var openSessionEvent = new RuntimeAnalyticsEvent(SESSION_OPENED_EVENT_NAME, SYSTEM_CATEGORY);
                TrackEvent(openSessionEvent);
            }
            
            UpdateSessionOpenedTimeStamp();
        }

        private void UpdateSessionOpenedTimeStamp()
        {
            m_sessionOpenedTimeStampInSeconds = DateTime.UtcNow;
            PlayerPrefs.SetString(SESSION_OPENED_TIME_STAMP_KEY, m_sessionOpenedTimeStampInSeconds.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
        }
        
        #region Unity Lifecycle
        private void Awake()
        {
            if (!m_initialized)
            {
                return;
            }
            
            var applicationAwakenEvent = new RuntimeAnalyticsEvent(APPLICATION_AWAKEN_EVENT_NAME, SYSTEM_CATEGORY);
            TrackEvent(applicationAwakenEvent);
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!m_initialized)
            {
                return;
            }
            
            var values = new Dictionary<string, object>() {{"focused", hasFocus}};
            var applicationFocusedEvent = new RuntimeAnalyticsEvent(APPLICATION_FOCUSED_EVENT_NAME, SYSTEM_CATEGORY,values);
            TrackEvent(applicationFocusedEvent);
        }

        private void OnApplicationQuit()
        {
            if (!m_initialized)
            {
                return;
            }
            
            var applicationQuitEvent = new RuntimeAnalyticsEvent(APPLICATION_QUIT_EVENT_NAME, SYSTEM_CATEGORY);
            TrackEvent(applicationQuitEvent);
            
            #if UNITY_EDITOR
            // I want to stop sending events when exiting play mode.
            Dispose();
            #endif
        }
        
        /// <summary>
        /// This callback is 'public' just to use it on Unit Tests
        /// </summary>
        /// <param name="paused"></param>
        public void OnApplicationPaused(bool paused)
        {
            if (!m_initialized)
            {
                return;
            }
            
            var values = new Dictionary<string, object>() {{"paused", paused}};
            var applicationPausedEvent = new RuntimeAnalyticsEvent(APPLICATION_PAUSED_EVENT_NAME, SYSTEM_CATEGORY,values);
            applicationPausedEvent.SkipStorageAndSyncDispatch = true;
            TrackEvent(applicationPausedEvent);
            
            if (paused)
            {
                ThreadUtils.RunOnMainThreadAsync(UpdateSessionOpenedTimeStamp);
                
                m_eventSlotManager.ForceWriteAllSlotsInDisk();
            }
            else
            {
                CheckNewSession();
            }
        }
        
        #endregion

        /// <summary>
        /// Called to cancel ongoing events 
        /// </summary>
        public void Dispose()
        {
            m_eventDispatcher?.Dispose();
            m_eventDispatcher = null;
        }

        #region Homa Analytics Methods

        public void TrackEvent(RuntimeAnalyticsEvent runtimeAnalyticsEvent)
        {
            HomaAnalyticsTaskUtils.Consume(DoTrackEventAsync(runtimeAnalyticsEvent));
        }
        
        private async Task DoTrackEventAsync(RuntimeAnalyticsEvent runtimeAnalyticsEvent)
        {
            if (runtimeAnalyticsEvent == null)
            {
                Debug.LogWarning("Can't track null events.");
                return;
            }
            
            if (!m_initialized)
            {
                CheckFirstSessionOpened();
                StorePreInitializationEvent(runtimeAnalyticsEvent);
                return;
            }
            
            if (!m_analyticsEnabled)
            {
                return;
            }

            // TokenIdentifier & ManifestVersionId are static field.
            // I want to set them everytime I send an event 
            EventApiQueryModel.TokenIdentifier = m_analyticsOptions.TokenIdentifier;
            EventApiQueryModel.ManifestVersionId = m_analyticsOptions.ManifestVersionId;
            
            // Inject install id and session id to all events to avoid doing this everytime we create a new event object
            runtimeAnalyticsEvent.InstallId = GetInstallId();
            runtimeAnalyticsEvent.SessionId = GetSessionId();
            runtimeAnalyticsEvent.NTestingId = m_analyticsOptions.NTestingId;
            runtimeAnalyticsEvent.NTestingOverrideId = m_analyticsOptions.NTestingOverrideId;

            if (m_analyticsOptions.VerboseLogs)
            {
                Debug.Log($"[HomaAnalytics Event] Tracking: {runtimeAnalyticsEvent.EventName}: {runtimeAnalyticsEvent}");
            }
            
            if (!m_analyticsOptions.EventValidation 
                || IsEventValid(runtimeAnalyticsEvent))
            {
                await StoreEventBeforeDispatching(runtimeAnalyticsEvent);
            }
        }

        private async Task StoreEventBeforeDispatching(RuntimeAnalyticsEvent analyticsEvent)
        {
            if (analyticsEvent == null)
            {
                return;
            }
            
            Task<string> serializeTask; 
            if (analyticsEvent.SkipStorageAndSyncDispatch)
            {
                serializeTask = Task.Run(() => AsyncSerializeEvent(analyticsEvent));
                serializeTask.Wait();
            }
            else
            {
                serializeTask = AsyncSerializeEvent(analyticsEvent);
                await serializeTask;
            }
            
            var bodyAsJsonString = serializeTask.Result;

            if (bodyAsJsonString == null)
            {
                Debug.LogWarning($"[HomaAnalytics] EventValue is null");
                return;
            }
            
            if (bodyAsJsonString.Length > MAX_EVENT_VALUES_LENGTH)
            {
                Debug.LogWarning($"[HomaAnalytics] EventValues can't have more than {MAX_EVENT_VALUES_LENGTH} characters. Current: {bodyAsJsonString.Length}");
                return;
            }

            var pendingEvent = new PendingEvent(analyticsEvent.EventName,
                analyticsEvent.EventId,
                bodyAsJsonString);

            pendingEvent.SkipStorageAndSyncDispatch = analyticsEvent.SkipStorageAndSyncDispatch;

            if (analyticsEvent.SkipStorageAndSyncDispatch)
            {
                // Developer Notes:
                // In order to try to save as much time as possible, we will skip the event storage for the pause event.
                // this is risky because if the event is lost, we won't send it again.
                // we want to measure first and then decide if we need to improve the solution storing and dispatching the event at the same time,
                // doing this, we can would have duplicated events in the server but this should be a minor issue compared to losing events.    
                m_eventDispatcher.DispatchPendingEvent(pendingEvent);
            }
            else
            {
                m_eventSlotManager.StoreEvent(pendingEvent);
            }
        }

        private async Task<string> AsyncSerializeEvent(RuntimeAnalyticsEvent analyticsEvent)
        {
            string bodyAsJsonString = null;
            var dictionary = analyticsEvent.ToDictionary();

            if (m_analyticsOptions.RecordAllEventsInCsv)
            {
                HomaAnalyticsEventRecorderDebugTool.RecordInCsv(m_persistentDataPath,
                    GetSessionId(),
                    analyticsEvent.EventId,
                    in dictionary);
            }
            
            await Task.Run(delegate
            {
                try
                {
                    bodyAsJsonString = Json.Serialize(dictionary);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ERROR] EventDispatcher: {e}");
                }
            });

            return bodyAsJsonString;
        }

        private bool IsEventValid(RuntimeAnalyticsEvent runtimeAnalyticsEvent)
        {
            if (runtimeAnalyticsEvent.InstallId != GetInstallId())
            {
                Debug.LogWarning($"[HomaAnalytics] Event install id: {runtimeAnalyticsEvent.InstallId} is different to the current install id: {GetInstallId()}. This only can happen if the game data was partially deleted.");
            }
            
            if (!VALID_CATEGORIES.ContainsKey(runtimeAnalyticsEvent.EventCategory))
            {
                Debug.LogWarning($"[HomaAnalytics] Can't recognize category: {runtimeAnalyticsEvent.EventCategory} in event: {runtimeAnalyticsEvent.EventName}.");
            }
            
            if (string.IsNullOrWhiteSpace(runtimeAnalyticsEvent.EventName))
            {
                Debug.LogError("[HomaAnalytics] EventName must be set.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(runtimeAnalyticsEvent.InstallId))
            {
                Debug.LogError($"[HomaAnalytics] InstallId can't be null or empty in event: {runtimeAnalyticsEvent.EventName}");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(runtimeAnalyticsEvent.SessionId))
            {
                Debug.LogError($"[HomaAnalytics] SessionId can't be null or empty in event: {runtimeAnalyticsEvent.EventName}");
                return false;
            }

            if (runtimeAnalyticsEvent.EventName.Length > MAX_EVENT_NAME_LENGTH)
            {
                Debug.LogError($"[HomaAnalytics] EventName can't have a length major than {MAX_EVENT_NAME_LENGTH} characters.");
                return false;
            }

            return true;
        }

        public string GetInstallId()
        {
            return m_installId;
        }

        public string GetSessionId()
        {
            return m_sessionId;
        }

        private void StorePreInitializationEvent(RuntimeAnalyticsEvent runtimeAnalyticsEvent)
        {
            Debug.Log($"[HomaAnalytics] Event {runtimeAnalyticsEvent.EventName} will be sent after HomaAnalytics was initialized.");
            // TODO: Should I store this in the file system as well as regular events?
            // Note: Storing the event ant not a copy can create issues if we decide to optimize
            // the memory allocation on the event creation
            m_preInitializationEvents.Enqueue(runtimeAnalyticsEvent);
        }

        /// <summary>
        /// This will clear all persistent data related with HA.
        /// </summary>
        public static void ClearPersistentData()
        {
            PlayerPrefs.DeleteKey(INSTALL_ID_KEY);
            PlayerPrefs.DeleteKey(SESSION_ID_KEY);
            PlayerPrefs.DeleteKey(SESSION_OPENED_TIME_STAMP_KEY);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Get useful debug info in formated text
        /// </summary>
        public string GetDebugInfo()
        {
            if (m_eventSlotManager == null)
            {
                return "Not initialized";
            }
            
            m_stringBuilderForDebugInfo.Clear();
            m_stringBuilderForDebugInfo.AppendLine($"Initialized: {Initialized}");
            m_stringBuilderForDebugInfo.AppendLine($"ASID: {Identifiers.Asid}");
            m_stringBuilderForDebugInfo.AppendLine($"GAID: {Identifiers.Gaid}");
            m_stringBuilderForDebugInfo.AppendLine($"IDFA: {Identifiers.Idfa}");
            m_stringBuilderForDebugInfo.AppendLine($"IDFV: {Identifiers.Idfv}");
            m_stringBuilderForDebugInfo.AppendLine($"DeviceId: {Identifiers.DeviceId}");
            m_stringBuilderForDebugInfo.AppendLine($"HomaGamesId: {Identifiers.HomaGamesId}");
            
            // Group and count states
            var activeSlotsCount = 0;
            var slotSummary = m_eventSlotManager.GetSlotsSummary();
            var statesCount = new Dictionary<string, int>();
            for (var index = 0; index < slotSummary.Count; index++)
            {
                if (index == 0)
                {
                    // First slot is always the active slot
                    m_stringBuilderForDebugInfo.AppendLine("Active Slot: "+slotSummary[index]);
                }
                
                var summary = slotSummary[index];
                if (statesCount.ContainsKey(summary.CurrentState.ToString()))
                {
                    statesCount[summary.CurrentState.ToString()]++;
                }
                else
                {
                    statesCount.Add(summary.CurrentState.ToString(), 1);
                }

                if (summary.CurrentState != EventStorageSlotStates.AllEventsDispatched)
                {
                    activeSlotsCount++;
                }
            }

            m_stringBuilderForDebugInfo.AppendLine($"Working Slots: {activeSlotsCount}");

            foreach (var stateCount in statesCount)
            {
                m_stringBuilderForDebugInfo.AppendLine($"SlotsState: {stateCount.Key}={stateCount.Value}");
            }

            m_stringBuilderForDebugInfo.AppendLine($"Slots Index: {m_eventSlotManager.GetUsedSlotCount()}");
            m_stringBuilderForDebugInfo.AppendLine($"Saved Events: {m_eventSlotManager.GetTotalSavedEvents()}");
            m_stringBuilderForDebugInfo.AppendLine($"Dispatched Events: {m_eventSlotManager.GetTotalDispatchedEvents()}");
            m_stringBuilderForDebugInfo.AppendLine($"{m_eventSlotManager.GetTotalSizeInBytes()}bytes");


            return m_stringBuilderForDebugInfo.ToString();
            
        }
        
        #endregion
        
        #region Homa Analytics Component Proxy
        
        /// <summary>
        /// Proxy GameObject to capture Unity lifecycle callbacks
        /// and keep track of them.
        ///
        /// This lifecycle callbacks track events within a `Task` so we make
        /// sure the event is fired to the engines when the app is being unfocused/paused
        /// </summary>
        public class HomaAnalyticsComponentProxy : MonoBehaviour
        {
            public HomaAnalytics HomaAnalyticsInstance { get; set; }

            private void Awake()
            {
                HomaAnalyticsInstance?.Awake();
            }

            private void OnApplicationQuit()
            {
                HomaAnalyticsInstance?.OnApplicationQuit();
            }

            private void OnApplicationPause(bool pauseStatus)
            {
                HomaAnalyticsInstance?.OnApplicationPaused(pauseStatus);
            }
        }
        
        #endregion
    }
}
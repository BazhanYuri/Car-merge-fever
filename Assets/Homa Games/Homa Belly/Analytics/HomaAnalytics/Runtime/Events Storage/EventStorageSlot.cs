using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class EventStorageSlot
    {
        private const int DEFAULT_SLOT_SIZE = 10;
        private readonly string m_completePath = null;
        private readonly int m_maxEvents = 0;
        private readonly List<PendingEvent> m_eventsToSave = null;
        /// <summary>
        /// Memory representation of events already stored in the file system. 
        /// </summary>
        private readonly List<PendingEvent> m_savedEvents = null;
        private readonly EventDispatcher m_eventDispatcher = null;
        private readonly int m_millisecondsToAutoDispatch = 0;
        private Stream m_stream = null;
        private bool m_closed = false;
        private bool m_disposed = false;
        private int m_eventsBeingSavedCount = 0;
        private int m_eventsSavedCount = 0;
        private int m_dispatchedEventsCount = 0;
        private bool m_dispatchCalled = false;
        private bool m_autoDispatchIfLimitReached = false;
        private bool m_autoDispatchByTimeTriggered = false;

        #region Lock Objects
        private readonly object m_closedLock = new object();
        private readonly object m_savedEventsLock = new object();
        private readonly object m_stateLock = new object();
        private readonly object m_eventsToSaveLock = new object();
        private readonly object m_dispatchCalledLock = new object();
        private readonly object m_disposedLock = new object();
        private readonly object m_fileStreamLock = new object();
        #endregion
        public EventStorageSlotStates State { get; private set; } = EventStorageSlotStates.Idle;
        /// <summary>
        /// Path to the file in which events of this slot are stored.
        /// </summary>
        public string CompleteFilePath => m_completePath;
        
        public string SlotName { get; }
        
        /// <summary>
        /// Total amount of bytes of serialized events. This doesn't represent disk space.
        /// Event bytes aren't represented here until Write is executed, so you can have some delay between
        /// an event is added ad its bytes are represented here. 
        /// </summary>
        public int SizeInBytes { get; private set; }

        /// <summary>
        /// Create a event storage slot
        /// </summary>
        /// <param name="eventDispatcher">Class to dispatch events to the server</param>
        /// <param name="directoryPath">Directory Path in which the slot file will be created.</param>
        /// <param name="fileNameWithExtension">Name of the slot file.</param>
        /// <param name="maxSize">Max amount of events this slot can store. We will flush the slot if the maximum is reached</param>
        /// <param name="millisecondsToAutoDispatch">We will dispatch the slot automatically after the first event is added after the time indicated by this parameter.</param>
        /// <param name="autoDispatchIfLimitReached">If true, we will flush the slot automatically if the maximum of events is reached.</param>
        public EventStorageSlot(EventDispatcher eventDispatcher,
            string directoryPath,
            string fileNameWithExtension,
            int maxSize,
            int millisecondsToAutoDispatch,
            bool autoDispatchIfLimitReached = false)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (maxSize == 0)
            {
                Debug.LogError($"[ERROR] Can't setup a slot with size 0. Default size will be used: {DEFAULT_SLOT_SIZE}");
                maxSize = DEFAULT_SLOT_SIZE;
            }

            m_maxEvents = maxSize;
            m_completePath = Path.Combine(directoryPath,fileNameWithExtension);
            var nameAndExtension = fileNameWithExtension.Split('.');
            SlotName = nameAndExtension.Length > 0 ? nameAndExtension[0] : fileNameWithExtension;
            m_savedEvents = new List<PendingEvent>(maxSize);
            m_eventsToSave = new List<PendingEvent>(maxSize);
            m_eventDispatcher = eventDispatcher;
            m_autoDispatchIfLimitReached = autoDispatchIfLimitReached;
            m_millisecondsToAutoDispatch = millisecondsToAutoDispatch;

            if (File.Exists(m_completePath))
            {
                HomaAnalyticsTaskUtils.Consume(ReadEventsAndInitializeSlot());
            }
        }
        
        /// <summary>
        /// Try to store the event. Can fail if the slot is full or closed.
        /// </summary>
        public bool TryToStoreEvent(PendingEvent pendingEvent)
        {
            if (IsClosed())
            {
                HomaGamesLog.Warning(
                    $"[WARNING] EventStorageSlot is closed, cannot store event in this slot. Slot path: {m_completePath}");
                return false;
            }

            if (TotalEventsCount() >= m_maxEvents)
            {
                return false;
            }

            lock (m_fileStreamLock)
            {
                if (m_stream == null)
                {
                    m_stream = CreateFileStream(m_completePath);
                }
            }

            lock (m_eventsToSaveLock)
            {
                m_eventsToSave.Add(pendingEvent);
            }

            WritePendingEventsIfIdle();

            return true;
        }
        
        /// <summary>
        /// When a slot is closed, we can't write more events on it and the slot file is closed too.
        /// </summary>
        public void CloseSlot()
        {
            lock (m_closedLock)
            {
                if (IsClosed())
                {
                    return;
                }
                
                m_closed = true;
            }
            
            HomaAnalyticsTaskUtils.Consume(TryToDisposeStreamWhenClosed());
        }

        public bool IsClosed()
        {
            return m_closed;
        }
        
        /// <summary>
        /// Force to dispatch the slot.
        /// If you don't call this method, it will automatically dispatched if it contains events after some time.
        /// Or it will be automatically dispatched when the max event limit is reached (only in configured).
        /// You can call Dispatch anytime, but you can't use the slot after it has been dispatched.
        /// </summary>
        public async Task DispatchSlot()
        {
            lock (m_dispatchCalledLock)
            {
                if (m_dispatchCalled)
                {
                    HomaGamesLog.Warning($"[WARNING] Can't dispatch the slot twice: {this}.");
                    return;
                }

                m_dispatchCalled = true;
                // Stop auto dispatch flags if dispatch is called 
                m_autoDispatchByTimeTriggered = true;
                m_autoDispatchIfLimitReached = false;
            }
            
            // Close the slot to avoid having new events.
            if (!IsClosed())
            {
                CloseSlot();
            }

            // Wait until Write/Read operations are done.
            while (State == EventStorageSlotStates.Writing 
                   || State == EventStorageSlotStates.Reading
                   || State == EventStorageSlotStates.Disposing)
            {
                await Task.Delay(10);
            }

            if (GetSavedEventsCount() == 0)
            {
                HomaGamesLog.Warning($"[WARNING] Can't dispatch an empty slot: {this}. Slot will be removed");
                File.Delete(m_completePath);
                return;
            }

            ChangeState(EventStorageSlotStates.DispatchingEvents);

            lock (m_savedEventsLock)
            {
                for (int i = 0; i < m_savedEvents.Count; i++)
                {
                    var pendingEvent = m_savedEvents[i];
                    pendingEvent.NotifyOnDispatch(PendingEventDispatchedHandler);
                    m_eventDispatcher.DispatchPendingEvent(pendingEvent);
                }
            }
        }

        /// <summary>
        /// If called, the slot will stop writing events in the file system.
        /// Everything would keep in memory until it is dispatched.
        /// </summary>
        public bool BypassFileSystem()
        {
            if (State == EventStorageSlotStates.Writing || State == EventStorageSlotStates.Reading)
            {
                Debug.LogError($"[ERROR] Can't bypass filesystem in slot {SlotName} because the slot is using it: {State}");
                return false;
            }

            lock (m_fileStreamLock)
            {
                m_stream = CreateMemoryStream();
            }

            return true;
        }
        
        #region Read/Write methods

        private async Task ReadEventsAndInitializeSlot()
        {
            lock (m_fileStreamLock)
            {
                m_stream = CreateFileStream(m_completePath, true);
            }
            
            try
            {
                ChangeState(EventStorageSlotStates.Reading);
                
                // We store the events in this format:
                // EventNameSize(int32)EventNameEventIdSize(int32)EventIdEventBodySize(int32)EventBodyTotalLength(int32)

                
                // Each character in UTF8 can takes from 1 to 4 bytes (depending on the character)
                // so will multiply each character by 4 to have get the max size
                // https://wiki.sei.cmu.edu/confluence/display/c/MSC10-C.+Character+encoding%3A+UTF8-related+issues#:~:text=UTF%2D8%20uses%201%20to,both%20ASCII%20and%20UTF%2D8.

                while (m_stream.Position < m_stream.Length)
                {
                    // Read event name
                    var eventName = await ReadFieldFromFile(HomaAnalytics.MAX_EVENT_NAME_LENGTH * 4);

                    // Read event id, (GUID) => 36 characters
                    var eventId = await ReadFieldFromFile(36);

                    var eventBody = await ReadFieldFromFile(HomaAnalytics.MAX_EVENT_VALUES_LENGTH * 4);

                    // To avoid sending corrupted data, we will check at the end of each event the total event length
                    // We write this mark in the writing method.
                    var buffer = new byte[4];
                    var _ = await m_stream.ReadAsync(buffer, 0, buffer.Length);
                    var eventFieldSize = BitConverter.ToInt32(buffer, 0);
                    var totalLength = (eventName?.Length ?? 0) + (eventId?.Length ?? 0) + (eventBody?.Length ?? 0);
                    if (eventFieldSize != totalLength)
                    {
                        Debug.LogError($"[ERROR] End of the event {eventName} is invalid. The file is corrupted: {this}");
                        ClearStream();
                        return;
                    }
                    SizeInBytes += totalLength;
                    var pendingEvent = new PendingEvent(eventName, eventId, eventBody);
                    AddSavedEvent(pendingEvent);
                }
            }
            catch (Exception exception)
            {
                HomaGamesLog.Error($"[ERROR] Failed to read events from slot file: {exception}");
            }
            finally
            {
                ChangeState(EventStorageSlotStates.Idle);
            }
        }
        
        /// <summary>
        /// This method will write all pending events to the slot file.
        /// It will write events in batches instead of one by one to improve performance. 
        /// </summary>
        private void WritePendingEventsIfIdle()
        {
            lock (m_stateLock)
            {
                // Only allow one process at the same time in the slot
                if(State != EventStorageSlotStates.Idle)
                {
                    return;
                }
            }
            // If new events arrive while writing, we will write them later.
            ChangeState(EventStorageSlotStates.Writing);

            // Trick: Wait one millisecond to batch events together. Writing several events at once is faster than one by one
            HomaAnalyticsTaskUtils.Consume(Task.Delay(1).ContinueWithOnMainThread(async _ =>
            {
                // Move events to a temp list to avoid concurrent modification
                // more events can arrive while this batch is being written into disk
                var eventsBatchToSave = new List<PendingEvent>();
                lock (m_eventsToSaveLock)
                {
                    eventsBatchToSave.AddRange(m_eventsToSave);
                    m_eventsToSave.Clear();
                    m_eventsBeingSavedCount = eventsBatchToSave.Count;
                    if (HasEventsOverflow())
                    {
                        HomaGamesLog.Warning("[WARNING] Events overflow. We received more events than the max allowed: " + TotalEventsCount()+" VS "+m_maxEvents);
                    }
                }

                try
                {
                    await WriteBytesInStream(eventsBatchToSave);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[ERROR] Can't save slot: {SlotName}:{State} in the file system. They will be stored in memory instead {exception}. ");
                    lock (m_fileStreamLock)
                    {
                        m_stream = CreateMemoryStream();
                    }
                    await WriteBytesInStream(eventsBatchToSave);
                }
                finally
                {
                    AddSavedEvent(eventsBatchToSave.ToArray());
                    m_eventsBeingSavedCount = 0;
                    ChangeState(EventStorageSlotStates.Idle);
                }
            }));
        }

        private async Task WriteBytesInStream(List<PendingEvent> eventsBatchToSave)
        {
            var allEventBytes = new List<byte>();
            for (int i = 0; i < eventsBatchToSave.Count; i++)
            {
                var pendingEvent = eventsBatchToSave[i];
                int totalLength = 0;
                
                // Event name
                totalLength += AddField(pendingEvent.EventName, ref allEventBytes);
                
                // Event id
                totalLength += AddField(pendingEvent.Id, ref allEventBytes);

                // Event body
                totalLength += AddField(pendingEvent.Json, ref allEventBytes);

                // Write total length of all fields at the end of the event. We will use this
                // to validate data when reading
                SizeInBytes += totalLength;
                allEventBytes.AddRange(BitConverter.GetBytes(totalLength));
            }

            await m_stream.WriteAsync(allEventBytes.ToArray(), 0, allEventBytes.Count);
        }

        private static int AddField(string value, ref List<byte> data)
        {
            if (value != null)
            {
                var eventNameBytes = Encoding.UTF8.GetBytes(value);
                data.AddRange(BitConverter.GetBytes(eventNameBytes.Length));
                data.AddRange(eventNameBytes);
                return eventNameBytes.Length;
            }
            else
            {
                data.AddRange(BitConverter.GetBytes(-1));
                return 0;
            }
        }

        private async Task<string> ReadFieldFromFile(int maxFieldSize)
        {
            // Read field size
            byte[] buffer = new byte[4];
            var _ =await m_stream.ReadAsync(buffer, 0, buffer.Length);
            int eventFieldSize = BitConverter.ToInt32(buffer, 0);
            if (eventFieldSize < -1 || eventFieldSize > maxFieldSize)
            {
                Debug.LogError($"[ERROR] Event field size is invalid: {eventFieldSize} Max {maxFieldSize}: . Aborting slot initialization. The file may be corrupted");
                return null;
            }

            if (eventFieldSize == -1)
                return null;
            else if (eventFieldSize == 0)
                return string.Empty;
            
            // Read field using the size
            buffer = new byte[eventFieldSize];
            var __ = await m_stream.ReadAsync(buffer, 0, buffer.Length);
            var field = Encoding.UTF8.GetString(buffer);
            if (string.IsNullOrEmpty(field))
            {
                Debug.LogError(
                    $"[ERROR] Event field is invalid: {field} this slot will be discarded. Aborting slot initialization. The file may be corrupted");
                return null;
            }

            return field;
        }
        
        #endregion
        
        private void ChangeState(EventStorageSlotStates newState)
        {
            lock (m_stateLock)
            {
                if (newState == State)
                {
                    Debug.LogError($"[ERROR] You can't change to the same state: {newState} {SlotName}");
                    return;
                }
                
                State = newState;
            }

            // Check if there are things to do if the slot gets idle
            if (State == EventStorageSlotStates.Idle)
            {
                if (HasPendingEventsToWrite())
                {
                    // A new set of events were added while the slot was reading or writing
                    WritePendingEventsIfIdle();
                }
                else if (IsClosed() && !m_disposed)
                {
                    // Event closed while it was reading or writing
                    HomaAnalyticsTaskUtils.Consume(TryToDisposeStreamWhenClosed());
                }
                else if (m_autoDispatchIfLimitReached 
                         && GetSavedEventsCount() >= m_maxEvents
                         && !m_dispatchCalled)
                {
                    // Max events reached, start dispatching events
                    HomaAnalyticsTaskUtils.Consume(DispatchSlot());
                }
            }
        }

        private async Task TryToDisposeStreamWhenClosed()
        {
            if (m_stream == null)
            {
                return;
            }
            
            if (!m_closed)
            {
                Debug.LogError("[ERROR] Can't dispose a non closed slot.");
                return;
            }

            if (State == EventStorageSlotStates.Reading 
                || State == EventStorageSlotStates.Writing)
            {
                // It will be disposed when the slot is idle after the writing/reading operation finishes
                return;
            }
            
            lock (m_disposedLock)
            {
                if (m_disposed)
                {
                    return;
                }
                m_disposed = true;
            }
            
            ChangeState(EventStorageSlotStates.Disposing);
            
            try
            {
                // To avoid locking the main thread when Dispose is called, call FlushAsync before disposing.
                await m_stream.FlushAsync();
                m_stream.Dispose();
            }
            catch (IOException ioException)
            {
                Debug.LogWarning($"Error disposing the slot: {ioException}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"Error disposing the slot: {exception}");
            }
            
            ChangeState(EventStorageSlotStates.Idle);
        }
        
        #region Helper methods

        /// <summary>
        /// Instead of removing and creating a new file, we just reset the opened stream.
        /// </summary>
        private void ClearStream()
        {
            if (m_stream != null)
            {
                m_stream.Seek(0, SeekOrigin.Begin);
                m_stream.SetLength(0);
            }
        }
        
        private void AddSavedEvent(params PendingEvent[] pendingEvents)
        {
            lock (m_savedEventsLock)
            {
                m_savedEvents.AddRange(pendingEvents);
                m_eventsSavedCount += pendingEvents.Length;
            }

            // After adding an event, start the auto dispatch timer
            // doing this we won't hold events for too long in the device
            if (!m_autoDispatchByTimeTriggered)
            {
                m_autoDispatchByTimeTriggered = true;
                if (!m_dispatchCalled)
                {
                    DelayedDispatchSlot(m_millisecondsToAutoDispatch);
                }
            }
        }

        /// <summary>
        /// Return how many events are saved in this slot file.
        /// This doesn't in return the amount of events that are being written.
        /// </summary>
        public int GetSavedEventsCount()
        {
            return m_eventsSavedCount;
        }
        
        /// <summary>
        /// Return the total amounts of events of this slot.
        /// Note that this will return already saved events and pending events
        /// </summary>
        public int TotalEventsCount()
        {
            return m_eventsToSave.Count + m_eventsBeingSavedCount + m_eventsSavedCount;
        }
        
        public int GetDispatchedEventsCount()
        {
            return m_dispatchedEventsCount;
        }

        private bool HasPendingEventsToWrite()
        {
            return m_eventsToSave.Count > 0;
        }

        private static Stream CreateFileStream(string path, bool read = false)
        {
            Stream fileStream;
            try
            {
                fileStream = new FileStream(path,
                    FileMode.OpenOrCreate,
                    read ? FileAccess.ReadWrite : FileAccess.Write,
                    FileShare.None,
                    4096,
                    useAsync: true);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ERROR] Can't create the file stream for the slot. {e}");
                // To avoid branching the code,we just create a memory stream instead of the file stream 
                fileStream = CreateMemoryStream();
            }

            return fileStream;
        }

        private static Stream CreateMemoryStream()
        {
            return new MemoryStream(4096);
        }

        /// <summary>
        /// True when the total amount of events is major than the max amount of events.
        /// This can happen if events where added while initialization was happening and the slot previously had
        /// a file with existing events.
        /// The slot will function normally.
        /// </summary>
        public bool HasEventsOverflow()
        {
            return TotalEventsCount() > m_maxEvents;
        }

        public bool HasSpace()
        {
            return TotalEventsCount() < m_maxEvents;
        }

        public override string ToString()
        {
            return $"{State} :: {TotalEventsCount()} :: {m_completePath}";
        }
        
        #endregion
        
        private void DelayedDispatchSlot(int milliseconds)
        {
            HomaAnalyticsTaskUtils.Consume(Task.Delay(milliseconds).ContinueWithOnMainThread(async _ =>
            {
                if (!m_dispatchCalled)
                {
                    await DispatchSlot();
                }
            }));
        }

        private void PendingEventDispatchedHandler(PendingEvent pendingEvent)
        {
            m_dispatchedEventsCount++;

            if (m_dispatchedEventsCount < GetSavedEventsCount())
            {
                return;
            }
            
            // Slot dispatched completely!
            ChangeState(EventStorageSlotStates.AllEventsDispatched);

            HomaAnalyticsTaskUtils.Consume(Task.Run(delegate
            {
                if (File.Exists(m_completePath))
                {
                    File.Delete(m_completePath);
                }
                
                m_savedEvents.Clear();
            }));
        }
    }
}
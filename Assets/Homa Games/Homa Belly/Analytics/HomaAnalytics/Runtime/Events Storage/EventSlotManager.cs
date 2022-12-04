using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class EventSlotManager
    {
        private const string SLOT_NAME_PREFIX = "Slot";
        
        private readonly string m_path = null;
        private readonly int m_maxEventsPerSlot = 0;
        private readonly int m_millisecondsToAutoDispatch = 0;
        private readonly EventDispatcher m_eventDispatcher = null;
        private readonly object m_lock = new object();
        private readonly List<EventStorageSlot> m_closedSlots = new List<EventStorageSlot>();
        private readonly int m_maxSizeInBytes;
        private EventStorageSlot m_activeSlot = null; 
        private int m_nextSlotIndex = 0;
        

        public EventSlotManager(EventDispatcher eventDispatcher,
            string path,
            int maxEventsPerSlot,
            int millisecondsToAutoDispatch,
            int maxSizeInBytes)
        {
            m_eventDispatcher = eventDispatcher;
            m_path = path;
            m_maxEventsPerSlot = maxEventsPerSlot;
            m_millisecondsToAutoDispatch = millisecondsToAutoDispatch;
            m_maxSizeInBytes = maxSizeInBytes;

            lock (m_lock)
            {
                if (!Directory.Exists(m_path))
                {
                    Directory.CreateDirectory(m_path);
                }
                
                // Recover slots from the file system
                var slotFiles = Directory.GetFiles(m_path);
                m_nextSlotIndex = slotFiles.Length-1;
                for (int i = 0; i < slotFiles.Length; i++)
                {
                    string fileName = Path.GetFileName(slotFiles[i]);
                    if (int.TryParse(Regex.Match(fileName, @"\d+").Value,out var fileNumber))
                    {
                        m_nextSlotIndex = Math.Max(m_nextSlotIndex, fileNumber);
                    }
                    var slot = CreateNewSlot(fileName);
                    // Try to dispatch slots ASAP
                    HomaAnalyticsTaskUtils.Consume(slot.DispatchSlot());
                    m_closedSlots.Add(slot);
                }
                
                m_nextSlotIndex++;
            }
        }

        public void StoreEvent(PendingEvent pendingEvent)
        {
            // This can be called inside a Task.Run()
            EventStorageSlot slot;
            lock (m_lock)
            { 
                slot = SelectSlot();
            }
            
            if (! slot.TryToStoreEvent(pendingEvent))
            {
                Debug.LogError($"[ERROR] Failed to store event in slot. This error shouldn't happen. This class takes care of checking for closed slots: {slot}");
            }
        }

        private EventStorageSlot SelectSlot()
        {
            if (m_activeSlot == null)
            {
                m_activeSlot = CreateNewSlot();
            }
            else if(m_activeSlot != null)
            {
                if (!m_activeSlot.HasSpace() 
                    && !m_activeSlot.IsClosed())
                {
                    m_activeSlot.CloseSlot();
                }
                
                if (m_activeSlot.IsClosed())
                {
                    m_closedSlots.Add(m_activeSlot);
                    m_activeSlot = CreateNewSlot();
                }
            }

            return m_activeSlot;
        }

        /// <summary>
        /// To avoid losing data before the users quit the application, write everything on disk.
        /// </summary>
        public void ForceWriteAllSlotsInDisk()
        {
            if (m_activeSlot != null && !m_activeSlot.IsClosed())
            {
                // closing a slot will flush the buffers into disk
                m_activeSlot.CloseSlot();
                m_closedSlots.Add(m_activeSlot);
                m_activeSlot = null;
            }
        }
        
        private EventStorageSlot CreateNewSlot()
        {
            var newSlot = CreateNewSlot($"{SLOT_NAME_PREFIX}{m_nextSlotIndex}.json");
            m_nextSlotIndex++;
            
            var totalSizeInBytes = GetTotalSizeInBytes();
            
            // Because we don't need such level of precision,
            // We only check max size when a new slot is created, not when we add a new event.
            
            if (totalSizeInBytes >= m_maxSizeInBytes)
            {
                Debug.LogWarning($"[WARNING] Max allowed size reached: {totalSizeInBytes}bytes Max: {m_maxSizeInBytes}bytes. We will keep sending events, but we won't store them.");
                newSlot.BypassFileSystem();
            }
            
            return newSlot;
        }
        
        private EventStorageSlot CreateNewSlot(string fileName)
        {
            var slot = new EventStorageSlot(m_eventDispatcher,
                m_path,
                fileName,
                m_maxEventsPerSlot,
                m_millisecondsToAutoDispatch,
                true);

            return slot;
        }

        public int GetUsedSlotCount()
        {
            return m_nextSlotIndex;
        }
        
        public int GetTotalDispatchedEvents()
        {
            int sum = 0;
            if (m_activeSlot != null)
            {
                sum += m_activeSlot.GetDispatchedEventsCount();
            }

            for (int i = 0; i < m_closedSlots.Count; i++)
            {
                sum += m_closedSlots[i].GetDispatchedEventsCount();
            }

            return sum;
        }
        
        public int GetTotalSavedEvents()
        {
            int sum = 0;
            if (m_activeSlot != null)
            {
                sum += m_activeSlot.GetSavedEventsCount();
            }

            for (int i = 0; i < m_closedSlots.Count; i++)
            {
                sum += m_closedSlots[i].GetSavedEventsCount();
            }

            return sum;
        }

        /// <summary>
        /// Get size of all stored events. The value has some delay because isn't updated
        /// until slots are written into disk.
        /// This doesn't represent the size in disk, it represents the memory size of slots.
        /// </summary>
        public int GetTotalSizeInBytes()
        {
            int totalBytes = 0;
            if (m_activeSlot != null && m_activeSlot.State != EventStorageSlotStates.AllEventsDispatched)
            {
                totalBytes += m_activeSlot.SizeInBytes;
            }

            for (int i = 0; i < m_closedSlots.Count; i++)
            {
                if (m_closedSlots[i] != null && m_closedSlots[i].State != EventStorageSlotStates.AllEventsDispatched)
                {
                    totalBytes += m_closedSlots[i].SizeInBytes;
                }
            }

            return totalBytes;
        }

        #region Slot summary
        /// <summary>
        /// Get a summary of slots state in this momment.
        /// </summary>
        public List<SlotSummary> GetSlotsSummary()
        {
            var slotsSummary = new List<SlotSummary>(); 
            if (m_activeSlot != null)
            {
                slotsSummary.Add(new SlotSummary(m_activeSlot));
            }
            
            for (int i = 0; i < m_closedSlots.Count; i++)
            {
                slotsSummary.Add(new SlotSummary(m_closedSlots[i]));
            }

            return slotsSummary;
        }

        public class SlotSummary
        {
            public string SlotName { get; }
            public int DispatchedEvents { get; }
            public int TotalSavedEvents { get; }
            public EventStorageSlotStates CurrentState { get; }
            public bool IsClosed { get; }

            public SlotSummary(EventStorageSlot slot)
            {
                SlotName = slot.SlotName;
                DispatchedEvents = slot.GetDispatchedEventsCount();
                TotalSavedEvents = slot.GetSavedEventsCount();
                CurrentState = slot.State;
                IsClosed = slot.IsClosed();
            }

            public override string ToString()
            {
                return
                    $"{SlotName} Saved: {TotalSavedEvents} Dispatched: {DispatchedEvents} Closed:{IsClosed} State: {CurrentState}";
            }
        }
        #endregion
    }
}
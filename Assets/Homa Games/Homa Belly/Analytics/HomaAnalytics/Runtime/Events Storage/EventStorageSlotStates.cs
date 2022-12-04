namespace HomaGames.HomaBelly
{
    public enum EventStorageSlotStates
    {
        Idle, // We always go to idle after a operation finishes. We check in Idle if there is something to do next. 
        Writing, // Writing events in the file system. The slot can receive new events while writing
        Reading, // Only when we initialize the slot if a file exist with the same slot name. We read the file and store the events in the slot.
        Disposing, // Can happen when the slot is closed. We write the buffer from memory to the opened file.
        DispatchingEvents, // We start to post events to the server. We will stay in this state until all events are posted.
        AllEventsDispatched // Final state. The slot is useless after this state.
    }
}
using System;
using HomaGames.HomaBelly;

public class EventTask : TaskBase
{
    public EventTask(ref Action observedEvent)
    {
        observedEvent += OnTaskCompleted;
    } 
    
    // Because event Actions cannot be passed as ref Action
    public EventTask(Action<Action> observerConsumer)
    {
        observerConsumer.Invoke(OnTaskCompleted);
    } 
}

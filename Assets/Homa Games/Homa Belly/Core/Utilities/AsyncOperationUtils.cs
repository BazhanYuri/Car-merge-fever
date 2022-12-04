using System.Threading.Tasks;
using HomaGames.HomaBelly;
using UnityEngine;

public static class AsyncOperationUtils
{
    public static TaskBase.Awaitable GetAwaiter(this AsyncOperation asyncOperation)
    {
        return new AsyncOperationTask(asyncOperation).GetAwaiter();
    }
    
    private class AsyncOperationTask : TaskBase
    {
        public AsyncOperationTask(AsyncOperation operation)
        {
            operation.completed += _ => OnTaskCompleted();
        }
    }
}



using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class TaskUtils
    {
        public static void ListenForErrors(this Task voidedTask)
        {
            voidedTask.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogException(task.Exception);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
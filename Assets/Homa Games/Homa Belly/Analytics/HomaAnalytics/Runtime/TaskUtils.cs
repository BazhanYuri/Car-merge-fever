using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class HomaAnalyticsTaskUtils
    {
        public static void Consume(Task voidedTask)
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
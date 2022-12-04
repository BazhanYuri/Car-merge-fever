using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class HomaAnalyticsLogListener
    {
        private static readonly Queue<LogElement> LogElementBuffer = new Queue<LogElement>();

        private static Application.LogCallback LogMessageReceived;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterCallback()
        {
            Application.logMessageReceived += OnLogReceived;
        }

        public static void SetLogReceivedCallback(Application.LogCallback callback)
        {
            LogMessageReceived = callback;
            
            foreach (var logElement in LogElementBuffer)
            {
                LogMessageReceived.Invoke(
                    logElement.condition,
                    logElement.stacktrace,
                    logElement.type
                );
            }
            
            LogElementBuffer.Clear();
        }

        private static void OnLogReceived(string condition, string stacktrace, LogType type)
        {
            if (LogMessageReceived != null)
            {
                LogMessageReceived.Invoke(condition, stacktrace, type);
            }
            else
            {
                LogElementBuffer.Enqueue(
                    new LogElement { condition = condition, stacktrace = stacktrace, type = type }
                    );
            }
        }
        

        private struct LogElement
        {
            public string condition;
            public string stacktrace; 
            public LogType type;
        }
    }
}
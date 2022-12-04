using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using HomaGames.HomaBelly;
using UnityEngine;
using Object = UnityEngine.Object;

// This class is a hack to override default unity Debug.Log calls.
// When this class is enabled, it will disable all logs.
// The Conditional attribute will strip all methods and calls of the build, so we won't pay the cost
// of strings being created in a Debug.Log method
// Note that if some code uses the complete method path UnityEngine.Debug.Log it won't be stripped.
// In case you have issues with this class, you can use the define symbol HOMA_BELLY_DEBUG_CLASS_OVERRIDE_ENABLED
#if !UNITY_EDITOR && HOMA_BELLY_DEBUG_CLASS_OVERRIDE_ENABLED
public static class Debug
{
    public static ILogger unityLogger => UnityEngine.Debug.unityLogger;
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
    {
        UnityEngine.Debug.DrawLine(start, end, color, duration);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        UnityEngine.Debug.DrawLine(start, end, color);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void DrawLine(Vector3 start, Vector3 end)
    {
        UnityEngine.Debug.DrawLine(start, end);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
    {
        UnityEngine.Debug.DrawRay(start, dir, color, duration);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void DrawRay(Vector3 start, Vector3 dir, Color color)
    {
        UnityEngine.Debug.DrawRay(start, dir, color);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void DrawRay(Vector3 start, Vector3 dir)
    {
        UnityEngine.Debug.DrawRay(start, dir);
    }

    public static void Break()
    {
        UnityEngine.Debug.Break();
    }

    public static void DebugBreak()
    {
        UnityEngine.Debug.DebugBreak();
    }

    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void Log(object message)
    {
        UnityEngine.Debug.Log(message);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void Log(object message,Object context)
    {
        UnityEngine.Debug.Log(message,context);
    }

    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogFormat(string format, params object[] args)
    {
        UnityEngine.Debug.LogFormat(format,args);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogFormat(Object context, string format, params object[] args)
    {
        UnityEngine.Debug.LogFormat(context,format,args);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogFormat(
        LogType logType,
        LogOption logOptions,
        Object context,
        string format,
        params object[] args)
    {
       UnityEngine.Debug.LogFormat(logType,logOptions,context,format,args);
    }

    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogError(object message)
    {
        UnityEngine.Debug.LogError(message);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogError(object message,Object context)
    {
        UnityEngine.Debug.LogError(message,context);
    }

    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogErrorFormat(string format, params object[] args)
    {
        UnityEngine.Debug.LogErrorFormat(format,args);
    }

    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogErrorFormat(Object context, string format, params object[] args)
    {
        UnityEngine.Debug.LogErrorFormat(context,format,args);
    }

    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogException(Exception exception)
    {
        UnityEngine.Debug.LogException(exception);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogException(Exception exception,Object context)
    {
        UnityEngine.Debug.LogException(exception,context);
    }

    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogWarning(object message)
    {
        UnityEngine.Debug.LogWarning(message);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogWarning(object message, Object context)
    {
        UnityEngine.Debug.LogWarning(message,context);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogWarningFormat(string format, params object[] args)
    {
        UnityEngine.Debug.LogWarningFormat(format,args);
    }

    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogWarningFormat(Object context, string format, params object[] args)
    {
        UnityEngine.Debug.LogWarningFormat(context,format,args);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void Assert(bool condition)
    {
        UnityEngine.Debug.Assert(condition);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void Assert(bool condition, Object context)
    {
        UnityEngine.Debug.Assert(condition,context);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void Assert(bool condition, object message)
    {
        UnityEngine.Debug.Assert(condition,message);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void Assert(bool condition, string message)
    {
        UnityEngine.Debug.Assert(condition,message);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void Assert(bool condition, object message, Object context)
    {
        UnityEngine.Debug.Assert(condition,message,context);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void Assert(bool condition, string message, Object context)
    {
        UnityEngine.Debug.Assert(condition,message,context);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void AssertFormat(bool condition, string format, params object[] args)
    {
        UnityEngine.Debug.AssertFormat(condition,format,args);
    }
    
    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void AssertFormat(
        bool condition,
        Object context,
        string format,
        params object[] args)
    {
        UnityEngine.Debug.AssertFormat(condition,context,format,args);
    }

    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogAssertion(object message)
    {
        UnityEngine.Debug.LogAssertion(message);
    }

    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogAssertion(object message, Object context)
    {
        UnityEngine.Debug.LogAssertion(message,context);
    }

    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogAssertionFormat(string format, params object[] args)
    {
        UnityEngine.Debug.LogAssertionFormat(format,args);
    }

    [Conditional(HomaBelly.LOGS_ENABLED_DEFINE_SYMBOL)]
    public static void LogAssertionFormat(Object context, string format, params object[] args)
    {
        UnityEngine.Debug.LogAssertionFormat(context,format,args);
    }

    public static bool isDebugBuild => UnityEngine.Debug.isDebugBuild;
}
#endif
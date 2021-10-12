using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Violet
{
    public class VioletLogger
    {
        public static bool TimeLog = true;

        [Conditional("ENABLE_LOG")]
        public static void Log(object message)
        {
            if (TimeLog)
                message = string.Format("[{0}] {1}", Time.time.ToString(".00"), message);
            Debug.Log(message);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogWarning(object message)
        {
            if (TimeLog)
                message = string.Format("[{0}] {1}", Time.time.ToString(".00"), message);
            Debug.LogWarning(message);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogError(object message)
        {
            if (TimeLog)
                message = string.Format("[{0}] {1}", Time.time.ToString(".00"), message);
            Debug.LogError(message);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogFormat(string message, params object[] args)
        {
            if (TimeLog)
                message = string.Format("[{0}] {1}", Time.time.ToString(".00"), message);
            Debug.LogFormat(message, args);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogWarningFormat(string message, params object[] args)
        {
            if (TimeLog)
                message = string.Format("[{0}] {1}", Time.time.ToString(".00"), message);
            Debug.LogWarningFormat(message, args);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogErrorFormat(string message, params object[] args)
        {
            if (TimeLog)
                message = string.Format("[{0}] {1}", Time.time.ToString(".00"), message);
            Debug.LogErrorFormat(message, args);
        }

        [Conditional("ENABLE_LOG")]
        public static void LogException(string message, Exception ex)
        {
            if (TimeLog)
                message = string.Format("[{0}] {1}", Time.time.ToString(".00"), message);
            Debug.LogException(ex);
        }
    }
}
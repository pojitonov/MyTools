using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MyTools
{
    public static class Debug
    {
        public const string DefaultPrefix = "MyTools";

        private const string DebugChannelFormat = "{0} →";

        private static DebugConfig _config;

        private const string Log_Color = "#00FF66";
        private const string Warning_Color = "#FFCC00";
        private const string Error_Color = "#FF3333";
        private const string Exception_Color = "#CC33FF";

        public static void SetConfig(DebugConfig config)
        {
            _config = config;
            _config?.RebuildLookup();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ChannelOn(string channel)
        {
            if (string.IsNullOrEmpty(channel)) return true;
            if (!_config) return true;

            var on = _config.IsEnabled(channel);

#if UNITY_EDITOR
            if (!_config.IsEnabled(channel) && !HasChannel(channel))
            {
                _config.Set(channel, true);
                on = true;
            }

            bool HasChannel(string ch)
            {
                var list = _config.Channels;
                for (int i = 0; i < list.Count; i++)
                    if (list[i].name == ch)
                        return true;
                return false;
            }
#endif

            return on;
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(object message) => UnityEngine.Debug.Log(message);

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError(object message) => UnityEngine.Debug.LogError(message);

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning(object message) => UnityEngine.Debug.LogWarning(message);

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException(Exception exception) => UnityEngine.Debug.LogException(exception);

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(object message, UnityEngine.Object context) => UnityEngine.Debug.Log(message, context);

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError(object message, UnityEngine.Object context) =>
            UnityEngine.Debug.LogError(message, context);

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning(object message, UnityEngine.Object context) =>
            UnityEngine.Debug.LogWarning(message, context);

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException(Exception exception, UnityEngine.Object context) =>
            UnityEngine.Debug.LogException(exception, context);

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(string channel, string message)
        {
            if (!ChannelOn(channel)) return;
            string formattedChannel = string.Format(DebugChannelFormat, channel);
            UnityEngine.Debug.Log($"<color={Log_Color}>{formattedChannel}</color> {message}");
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(object source, string message)
        {
            string channel = source?.GetType().Name ?? "Unknown";
            Log(channel, message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning(string channel, string message)
        {
            if (!ChannelOn(channel)) return;
            string formattedChannel = string.Format(DebugChannelFormat, channel);
            UnityEngine.Debug.LogWarning($"<color={Warning_Color}>{formattedChannel}</color> {message}");
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogWarning(object source, string message)
        {
            string channel = source?.GetType().Name ?? "Unknown";
            LogWarning(channel, message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError(string channel, string message)
        {
            if (!ChannelOn(channel)) return;
            string formattedChannel = string.Format(DebugChannelFormat, channel);
            UnityEngine.Debug.LogError($"<color={Error_Color}>{formattedChannel}</color> {message}");
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogError(object source, string message)
        {
            string channel = source?.GetType().Name ?? "Unknown";
            LogError(channel, message);
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException(string channel, string message)
        {
            if (!ChannelOn(channel)) return;
            string formattedChannel = string.Format(DebugChannelFormat, channel);
            UnityEngine.Debug.LogWarning($"<color={Exception_Color}>{formattedChannel}</color> {message}");
        }

        [HideInCallstack, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogException(object source, string message)
        {
            string channel = source?.GetType().Name ?? "Unknown";
            LogException(channel, message);
        }
    }
}
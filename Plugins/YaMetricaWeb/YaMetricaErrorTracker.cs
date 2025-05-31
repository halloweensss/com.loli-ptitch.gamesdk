using System;
using UnityEngine;

namespace GameSDK.Plugins.YaMetricaWeb
{
    public class YaMetricaErrorTracker : IDisposable
    {
        private readonly YaMetricaWebAnalytics _analytics;
        private readonly System.Collections.Generic.HashSet<string> _recentErrors = new();

        public YaMetricaErrorTracker(YaMetricaWebAnalytics analytics)
        {
            _analytics = analytics;
        }
        
        public void Initialize()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            Application.logMessageReceived += HandleLog;   
        }

        private async void HandleLog(string condition, string stackTrace, LogType type)
        {
            if (type is LogType.Error or LogType.Exception or LogType.Assert)
            {
                var message = condition;
                var formattedStackTrace = stackTrace ?? "No stack trace available";
                var exceptionType = type.ToString();

                if (IsDuplicate(message, formattedStackTrace))
                    return;

                SendErrorEvent(message, formattedStackTrace, exceptionType);
            }
        }

        private async void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                var message = exception.Message;
                var stackTrace = exception.StackTrace ?? "No stack trace available";
                var type = exception.GetType().Name;

                if (IsDuplicate(message, stackTrace))
                    return;

                SendErrorEvent(message, stackTrace, type);
            }
        }

        private bool IsDuplicate(string message, string stackTrace)
        {
            string key = $"{message}:{stackTrace}";
            if (_recentErrors.Contains(key))
                return true;
            if (_recentErrors.Count > 100)
                _recentErrors.Clear();
            _recentErrors.Add(key);
            return false;
        }
        
        private void SendErrorEvent(string message, string stackTrace, string type)
        {
            _analytics.SendEvent("unity_error", new System.Collections.Generic.Dictionary<string, object>
            {
                { "message", message },
                { "stackTrace", stackTrace },
                { "type", type },
                { "device_model", SystemInfo.deviceModel },
                { "os", SystemInfo.operatingSystem },
                { "app_version", Application.version },
                { "device_type", SystemInfo.deviceType.ToString() },
                { "device_name", SystemInfo.deviceName },
                { "graphics_device", SystemInfo.graphicsDeviceName },
                { "graphics_memory", SystemInfo.graphicsMemorySize },
                { "system_memory", SystemInfo.systemMemorySize },
                { "processor", SystemInfo.processorType },
                { "platform", Application.platform.ToString() },
            });
        }
        
        public void Dispose()
        {
            AppDomain.CurrentDomain.UnhandledException -= HandleUnhandledException;
            Application.logMessageReceived -= HandleLog;
        }
    }
}
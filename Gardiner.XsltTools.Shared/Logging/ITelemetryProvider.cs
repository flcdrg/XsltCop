using System;

namespace Gardiner.XsltTools.Logging
{
    public interface ITelemetryProvider
    {
        void LogMessage(string message);
        void LogEvent(string eventName);
        void LogException(Exception ex);
        void Shutdown();
    }
}
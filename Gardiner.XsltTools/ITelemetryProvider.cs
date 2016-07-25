using System;

namespace Gardiner.XsltTools
{
    public interface ITelemetryProvider
    {
        void LogMessage(string message);
        void LogEvent(string eventName);
        void LogException(Exception ex);
        void Shutdown();
    }
}
using System;

using EnvDTE;

using EnvDTE80;

namespace Gardiner.XsltTools
{
    public static class Telemetry
    {
        private static ITelemetryProvider _provider;
        private static DTEEvents _events;

        public static void Initialise(ITelemetryProvider provider, DTE2 dte)
        {
            _provider = provider;
            _events = dte.Events.DTEEvents;

            _events.OnBeginShutdown += Shutdown;
        }

        public static void LogMessage(string message)
        {
            _provider.LogMessage(message);
        }

        public static void Log(Exception ex)
        {
            _provider.LogException(ex);
        }

        public static void Shutdown()
        {
            _provider.Shutdown();
        }
    }
}
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

using Microsoft.HockeyApp;
using Microsoft.HockeyApp.DataContracts;
using Microsoft.HockeyApp.Model;

namespace Gardiner.XsltTools
{
    public class HockeyClientTelemetryProvider : ITelemetryProvider
    {
        private HockeyClient _hockeyClient;
        private readonly Options _options;

        public HockeyClientTelemetryProvider(Options options)
        {
            _options = options;

            _options.PropertyChanged += OptionsOnPropertyChanged;

            Configure();
        }

        private void Configure()
        {
            if (_options.FeedbackAllowed)
            {
                _hockeyClient = (HockeyClient) HockeyClient.Current;

                if (string.IsNullOrEmpty(_hockeyClient.AppIdentifier))
                {
                    // We must only call Configure once
                    _hockeyClient.Configure("bb59fd06bb2a42aab4dff8125de22209")
                        .RegisterDefaultUnobservedTaskExceptionHandler();
                }
                var helper = new HockeyPlatformHelperWPF();

                var crashLogInfo = new CrashLogInformation()
                {
                    PackageName = helper.AppPackageName,
                    OperatingSystem = helper.OSPlatform,
                    Windows = _hockeyClient.OsVersion,
                    Manufacturer = helper.Manufacturer,
                    Model = VsVersion.FullVersion.ToString(), // Visual Studio version
                    ProductID = helper.ProductID,
                    Version = Assembly.GetExecutingAssembly().GetName().Version.ToString()
                };

                var field = typeof(HockeyClient).GetField("_crashLogInfo", BindingFlags.NonPublic | BindingFlags.Instance);

                field.SetValue(_hockeyClient, crashLogInfo);

#if DEBUG
                _hockeyClient.OnHockeySDKInternalException += (sender, args) =>
                {
                    if (Debugger.IsAttached) { Debugger.Break(); }
                };
#endif

                var telemetry = new EventTelemetry("Usage");
                telemetry.Properties.Add("OperatingSystem", crashLogInfo.OperatingSystem);
                telemetry.Properties.Add("Visual Studio", crashLogInfo.Model);
                telemetry.Properties.Add("Version", crashLogInfo.Version);
                _hockeyClient.TrackEvent(telemetry);

                _hockeyClient.Flush();
            }
            else
            {
                _hockeyClient = null;
            }
        }

        private void OptionsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(Options.FeedbackAllowed))
            {
                Configure();
            }
        }

        public void LogMessage(string message)
        {
            _hockeyClient?.TrackTrace(message);
        }

        public void LogEvent(string eventName)
        {
            _hockeyClient?.TrackEvent(eventName);
        }

        public void LogException(Exception ex)
        {
            // TrackException is not supported with WPF yet, so use HandleException instead
            _hockeyClient?.HandleException(ex);
        }

        public void Shutdown()
        {
            _hockeyClient?.Flush();
            Debug.WriteLine("Flushing telemetry");
        }
    }
}
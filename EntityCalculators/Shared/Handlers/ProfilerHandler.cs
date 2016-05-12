using Shared.Services;
using Shared.Services.LogListeners;
using System;
using System.Collections.Generic;

namespace Shared.Handlers
{
    public class ProfilerHandler : IProfilerHandler
    {
        private DateTime _startTime;
        private string _correlationId;
        private string _tag;
        private string _method;
        private double _duration;
        private Dictionary<string, string> _properties;

        private ILoggerService _loggerService;
        private ISettingService _settingService;

        //Logger Listeners
        AzureStorageLogListener _azureStorageListener;
        EtwLogListener _etwListener;

        public ProfilerHandler(ISettingService setting)
        {
            _loggerService = ServiceFactory.GetLoggerService();
            _settingService = setting;

            if (_settingService.IsAzureStorageLogging()) 
            {
                _azureStorageListener = new AzureStorageLogListener(_loggerService, _settingService);
            }

            if (_settingService.IsEtwLogging())
            {
                _etwListener = new EtwLogListener(_loggerService, _settingService);
            }
        }

        public void Start(string tag, string method, Dictionary<string, string> properties = null)
        {
            _startTime = DateTime.Now;
            _correlationId = Guid.NewGuid().ToString();
            _tag = tag;
            _method = method;
            var message = method + " - Started";
            _duration = 0;
            _properties = properties;
            _loggerService.Log(LogLevels.Severe, _correlationId, _tag, method, message, LogTypes.Start, _properties);
        }

        public void Info(string message)
        {
            _duration = 0;
            _loggerService.Log(LogLevels.Info, _correlationId, _tag, _method, _method + " - " + message, LogTypes.Info, _properties);
        }

        public void Error(string error)
        {
            _duration = 0;
            _loggerService.Log(LogLevels.Info, _correlationId, _tag, _method, _method + " caused an error: " + error, LogTypes.Error, _properties);
        }

        public void Stop(string error = "")
        {
            var message = _method + (string.IsNullOrEmpty(error) ? " - Ended" : " - Failed: " + error);
            _duration = (DateTime.Now - _startTime).TotalSeconds;
            _loggerService.Log(LogLevels.Severe, _correlationId, _tag, _method, message, (string.IsNullOrEmpty(error) ? LogTypes.Stop : LogTypes.AbnormalStop), _properties, _duration);

            try
            {
                if (_azureStorageListener != null)
                {
                    _azureStorageListener.Unsubscribe();
                }

                if (_etwListener != null)
                {
                    _etwListener.Unsubscribe();
                }
            }
            catch (Exception ex)
            {
                /* Ignore */
            }
        }
    }
}

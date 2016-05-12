using System;
using System.Collections.Generic;

namespace Shared.Services
{
    public class InsightsService : IInsightsService
    {
        private const string LOG_TAG = "InsightsService";

        private ISettingService _settingService;
        //private TelemetryClient _telemtryClient;

        public InsightsService(ISettingService setting)
        {
            _settingService = setting;
            if (!string.IsNullOrEmpty(_settingService.GetInstrumentationKey()))
            {
                //_telemtryClient = new TelemetryClient();
                //_loggerService.Log(LOG_TAG, "Key: {0}", _settingService.GetInstrumentationKey());
                //_telemtryClient.InstrumentationKey = _settingService.GetInstrumentationKey();
            }
        }

        public void TrackEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null)
        {
            //if (_telemtryClient != null)
            //    _telemtryClient.TrackEvent(eventName, properties, measurements);
        }

        public void TrackTrace(string message, Dictionary<string, string> properties = null)
        {
            //if (_telemtryClient != null)
            //    _telemtryClient.TrackTrace(message, properties);
        }

        public void TrackMetric(string metric, double value, Dictionary<string, string> properties = null)
        {
            //if (_telemtryClient != null)
            //    _telemtryClient.TrackMetric(metric, value, properties);
        }

        public void TrackException(Exception ex, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null)
        {
            //if (_telemtryClient != null)
            //    _telemtryClient.TrackException(ex, properties, measurements);
        }
    }
}

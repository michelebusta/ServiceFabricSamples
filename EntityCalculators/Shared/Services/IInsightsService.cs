using System;
using System.Collections.Generic;

namespace Shared.Services
{
    public interface IInsightsService
    {
        void TrackEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null);
        void TrackTrace(string message, Dictionary<string, string> properties = null);
        void TrackMetric(string metric, double value, Dictionary<string, string> properties = null);
        void TrackException(Exception ex, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null);
    }
}

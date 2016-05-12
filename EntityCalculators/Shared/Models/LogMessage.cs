using Shared.Services;
using System.Collections.Generic;

namespace Shared.Models
{
    public class LogMessage
    {
        public LogMessage(LogLevels level, string correlationId, string tag, string method, string message, LogTypes type, Dictionary<string, string> properties = null, double duration = 0)
        {
            Level = level;
            CorrelationId = correlationId;
            Tag = tag;
            Method = method;
            Message = message;
            Type = type;
            Properties = properties;
            Duration = duration;
        }

        public LogLevels Level { get; set; }
        public string CorrelationId { get; set; }
        public string Tag  { get; set; }
        public string Method { get; set; }
        public string Message { get; set; }
        public LogTypes Type { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public double Duration { get; set; }
    }
}

using Shared.Models;
using Shared.Services.LogSources;
using System;
using System.Collections.Generic;

namespace Shared.Services
{
    public class LoggerService : ILoggerService, ILogMessageEventSource
    {
        public event EventHandler<LogMessage> LogMessageReceived;

        public void Log(LogLevels level, string correlationId, string tag, string method, string message, LogTypes type, Dictionary<string, string> properties = null, double duration = 0)
        {
            // Convert to a dictionary
            LogMessageReceived?.Invoke(this, new LogMessage(level, correlationId, tag, method, message, type, properties, duration));
        }
    }
}

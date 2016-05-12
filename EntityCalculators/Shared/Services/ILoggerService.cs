using Shared.Services.LogSources;
using System.Collections.Generic;

namespace Shared.Services
{
    public interface ILoggerService : ILogMessageEventSource
    {
        void Log(LogLevels level, string correlationId, string tag, string method, string message, LogTypes type, Dictionary<string, string> properties = null, double duration = 0);
    }

    public enum LogLevels
    {
        Debug,
        Info,
        Medium,
        Severe
    }

    public enum LogTypes
    {
        Start,
        Info,
        Error,
        Stop,
        AbnormalStop
    }
}

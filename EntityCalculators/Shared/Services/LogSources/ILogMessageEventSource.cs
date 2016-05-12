using Shared.Models;
using System;

namespace Shared.Services.LogSources
{
    public interface ILogMessageEventSource
    {
        event EventHandler<LogMessage> LogMessageReceived;
    }
}

using System.Collections.Generic;

namespace Shared.Handlers
{
    public interface IProfilerHandler
    {
        void Start(string tag, string method, Dictionary<string, string> properties = null);
        void Info(string message);
        void Error(string error);
        void Stop(string error = "");
    }
}

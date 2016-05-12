using Shared.Services;

namespace Shared.Handlers
{
    public class HandlersFactory
    {
        public static IProfilerHandler GetProfilerHandler(ISettingService setting)
        {
            return new ProfilerHandler(setting);
        }
    }
}

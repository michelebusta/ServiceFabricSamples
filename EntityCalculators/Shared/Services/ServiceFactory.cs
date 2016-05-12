using System.Fabric;

namespace Shared.Services
{
    public class ServiceFactory
    {
        public static IActorLocationService GetActorLocationService()
        {
            return new ActorLocationService();
        }

        public static IServiceLocationService GetServiceLocationService()
        {
            return new ServiceLocationService();
        }

        public static IUriBuilderService GetUrilBuilderService(string serviceInstance)
        {
            return new UriBuilderService(serviceInstance);
        }

        public static IUriBuilderService GetUrilBuilderService(string applicationInstance, string serviceInstance)
        {
            return new UriBuilderService(applicationInstance, serviceInstance);
        }

        public static ILoggerService GetLoggerService()
        {
            return new LoggerService();
        }

        public static ISettingService GetSettingService()
        {
            return new SettingService();
        }

        public static IInsightsService GetInsightsService(ISettingService setting)
        {
            return new InsightsService(setting);
        }

        public static IOltpConnectorService GetOltpConnectorService(ISettingService setting)
        {
            var connectorType = setting.GetOltpConnectionType();
            if (!string.IsNullOrEmpty(connectorType) && connectorType.ToLower() == "fake")
                return new FakeConnectorService(setting);
            else
                return new RealConnectorService(setting);
        }
    }
}

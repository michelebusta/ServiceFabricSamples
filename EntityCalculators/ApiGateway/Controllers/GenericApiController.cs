using Shared.Helpers;
using Shared.Services;
using System.Collections.Generic;
using System.Fabric;
using System.Web.Http;

namespace ApiGateway.Controllers
{
    public class GenericApiController : ApiController
    {
        private ISettingService _settingService;

        public GenericApiController()
        {
            // This is really neat! The FabricRuntime.GetActivationContext() gives me access to the config context!!! :-)
            _settingService = ServiceFactory.GetSettingService();
        }

        public ISettingService TheSettingService
        {
            get { return _settingService; }
            set { _settingService = value; }
        }

        // TODO: If there is a way to access the SF context from the controller, we will change this
        protected Dictionary<string, string> GetServiceProperties()
        {
            Dictionary<string, string> props = new Dictionary<string, string>();
            props.Add(Constants.ServicePropType, "ApiGateway.ApiGateway");
            props.Add(Constants.ServicePropId, "API");
            props.Add(Constants.ServicePropApplicationType, "EntityCalculatorsAppType");
            props.Add(Constants.ServicePropApplicationName, "EntityCalculatorsApp");
            props.Add(Constants.ServicePropServiceType, "APIGatewayType");
            props.Add(Constants.ServicePropServiceName, "APIGateway");
            props.Add(Constants.ServicePropPartitionId, "API Gateway");
            props.Add(Constants.ServicePropReplicationId, "API Gateway");
            props.Add(Constants.ServicePropNode, "API Gateway");
            return props;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Shared.Helpers;

namespace ApiGateway
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class ApiGateway : StatelessService
    {
        public ApiGateway(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext => new OwinCommunicationListener(Startup.ConfigureApp, serviceContext, ServiceEventSource.Current, "ServiceEndpoint"))
            };
        }

        // PRIVATE
        public Dictionary<string, string> GetServiceProperties()
        {
            Dictionary<string, string> props = new Dictionary<string, string>();
            props.Add(Constants.ServicePropType, GetType().ToString());
            props.Add(Constants.ServicePropId, Context.ReplicaOrInstanceId.ToString());
            props.Add(Constants.ServicePropApplicationType, Context.CodePackageActivationContext.ApplicationTypeName);
            props.Add(Constants.ServicePropApplicationName, Context.CodePackageActivationContext.ApplicationName);
            props.Add(Constants.ServicePropServiceType, Context.ServiceTypeName);
            props.Add(Constants.ServicePropServiceName, Context.ServiceName.ToString());
            props.Add(Constants.ServicePropPartitionId, Context.PartitionId + "");
            props.Add(Constants.ServicePropReplicationId, Context.ReplicaOrInstanceId + "");
            props.Add(Constants.ServicePropNode, Context.NodeContext.NodeName);
            return props;
        }
    }
}

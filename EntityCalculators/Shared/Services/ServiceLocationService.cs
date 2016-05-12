using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;

namespace Shared.Services
{
    public class ServiceLocationService : IServiceLocationService
    {
        public TServiceInterface Create<TServiceInterface>(Uri serviceName) where TServiceInterface : IService
        {
            return ServiceProxy.Create<TServiceInterface>(serviceName);
        }

        public TServiceInterface Create<TServiceInterface>(long partitionKey, Uri serviceName) where TServiceInterface : IService
        {
            return ServiceProxy.Create<TServiceInterface>(serviceName, new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionKey));
        }

        public TServiceInterface Create<TServiceInterface>(string partitionKey, Uri serviceName) where TServiceInterface : IService
        {
            return ServiceProxy.Create<TServiceInterface>(serviceName, new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(partitionKey));
        }
    }
}

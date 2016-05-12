using Microsoft.ServiceFabric.Services.Remoting;
using System;

namespace Shared.Services
{
    public interface IServiceLocationService
    {
        TServiceInterface Create<TServiceInterface>(Uri serviceName) where TServiceInterface : IService;
        TServiceInterface Create<TServiceInterface>(string partitionKey, Uri serviceName) where TServiceInterface : IService;
        TServiceInterface Create<TServiceInterface>(long partitionKey, Uri serviceName) where TServiceInterface : IService;
    }
}

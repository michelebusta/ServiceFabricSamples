using Microsoft.ServiceFabric.Actors;

namespace Shared.Services
{
    public interface IActorLocationService
    {
        TActorInterface Create<TActorInterface>(long actorId, string applicationName) where TActorInterface : IActor;
        TActorInterface Create<TActorInterface>(string actorId, string applicationName) where TActorInterface : IActor;
    }
}

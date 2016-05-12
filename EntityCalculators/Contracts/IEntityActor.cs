using Microsoft.ServiceFabric.Actors;
using Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts
{
    /// <summary>
    /// This interface represents the actions a client app can perform on an actor.
    /// It MUST derive from IActor and all methods MUST return a Task.
    /// </summary>
    public interface IEntityActor : IActor, IActorEventPublisher<IEntityActorEvents>
    {
        Task Process(EntityTransaction transaction);
        Task<Entity> GetEntity();
        Task<Entity> GetParent();
        Task<List<Entity>> GetChildren();
        Task<EntityView> GetView();
        Task<int> GetPurchases();
        Task<int> GetCancellations();
        Task<int> GetItemsSold();
        Task<double> GetRevenue();
        Task<double> GetTax();
        Task<double> GetShipping();
    }
}

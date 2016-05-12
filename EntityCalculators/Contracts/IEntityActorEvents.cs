using Microsoft.ServiceFabric.Actors;
using Shared.Models;

namespace Contracts
{
    public interface IEntityActorEvents : IActorEvents
    {
        void MeasuresRecalculated(Entity entity, int purchases, int cancellations, int soldItems, double revenue, double tax, double shipping);
    }
}

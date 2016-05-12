using Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared.Services
{
    /// <summary>
    /// This interface provides access to the OLTP back-end system.  
    /// </summary>
    public interface IOltpConnectorService
    {
        Task<List<Entity>> GetEntities();
        Task<Entity> GetEntity(EntityTypes type, int businessKey);
        Task<Entity> GetParentEntity(EntityTypes type, int businessKey);
        Task<List<Entity>> GetChildren(EntityTypes type, int entityId);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Models;
using System.Linq;

namespace Shared.Services
{
    /// <summary>
    /// OLTP Connector that is aware of the different entities in the system.
    /// Currently this is identical to the real connector
    /// </summary>
    public class FakeConnectorService : IOltpConnectorService
    {
        //TODO: Add a caching layer
        private List<Entity> _entities = new List<Entity>();

        private ISettingService _settingService;
        private string _connectionString;

        public FakeConnectorService(ISettingService setting)
        {
            _settingService = setting;
            _connectionString = _settingService.GetOltpConnectionString();

            // Initialize Storage
            Initialize();
        }

        public Task<List<Entity>> GetEntities()
        {
            return Task.FromResult(_entities);
        }

        public Task<Entity> GetEntity(EntityTypes type, int businessKey)
        {
            var entity = _entities.Where(e => e.Type == type && e.BusinessKey == businessKey).FirstOrDefault();
            return Task.FromResult(entity);
        }

        public Task<Entity> GetParentEntity(EntityTypes type, int businessKey)
        {
            Entity parent = null;
            var entity = _entities.Where(e => e.Type == type && e.BusinessKey == businessKey).FirstOrDefault();
            if (entity != null)
                parent = entity.Parent;

            return Task.FromResult(parent);
        }

        public Task<List<Entity>> GetChildren(EntityTypes type, int businessKey)
        {
            return Task.FromResult(_entities.Where(e => e.Parent != null && e.Parent.Type == type && e.Parent.BusinessKey == businessKey).ToList());
        }

        // PRIVATE
        private void Initialize()
        {
            _entities = new List<Entity>();

            var global = new Entity() { Type = EntityTypes.Global, BusinessKey = 0, Name = "Global", Parent = null };

            var americas = new Entity() { Type = EntityTypes.Region, BusinessKey = 10, Name = "Americas", Parent = global };
            var usa = new Entity() { Type = EntityTypes.Country, BusinessKey = 20, Name = "USA", Parent = americas };
            var newYork = new Entity() { Type = EntityTypes.SalesOffice, BusinessKey = 30, Name = "New York", Parent = usa };
            var boston = new Entity() { Type = EntityTypes.SalesOffice, BusinessKey = 31, Name = "Boston", Parent = usa };

            var europe = new Entity() { Type = EntityTypes.Region, BusinessKey = 110, Name = "Europe", Parent = global };
            var uk = new Entity() { Type = EntityTypes.Country, BusinessKey = 120, Name = "UK", Parent = europe };
            var london = new Entity() { Type = EntityTypes.SalesOffice, BusinessKey = 130, Name = "London", Parent = uk };
            var manchester = new Entity() { Type = EntityTypes.SalesOffice, BusinessKey = 131, Name = "Manchester", Parent = uk };
            var france = new Entity() { Type = EntityTypes.Country, BusinessKey = 140, Name = "France", Parent = europe };
            var paris = new Entity() { Type = EntityTypes.SalesOffice, BusinessKey = 141, Name = "Paris", Parent = france };

            _entities.Add(global);
            _entities.Add(americas);
            _entities.Add(usa);
            _entities.Add(newYork);
            _entities.Add(boston);
            _entities.Add(europe);
            _entities.Add(uk);
            _entities.Add(london);
            _entities.Add(manchester);
            _entities.Add(france);
            _entities.Add(paris);
        }
    }
}

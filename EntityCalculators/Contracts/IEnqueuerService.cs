using Microsoft.ServiceFabric.Services.Remoting;
using Shared.Models;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IEnqueuerService : IService
    {
        Task<bool> EnqueueProcess(EntityTransaction transaction);
    }
}

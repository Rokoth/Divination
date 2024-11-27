using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public interface IAllocateReservesService
    {
        Task Execute(CancellationToken token);
    }
}
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public interface IReservesRevisorService
    {
        Task CheckReserveValues(CancellationToken token);
        Task CheckSum(CancellationToken token);
    }
}
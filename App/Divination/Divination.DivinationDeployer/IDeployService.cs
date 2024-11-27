using System.Threading.Tasks;

namespace Divination.DivinationDeployer
{
    public interface IDeployService
    {
        Task Deploy(int? num = null);
    }
}

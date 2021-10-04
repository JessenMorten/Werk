using System.Threading.Tasks;

namespace Werk.Services
{
    public interface IWerkService
    {
        Task<WerkServiceStatus> GetStatus();
    }
}

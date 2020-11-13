using JumpenoWebassembly.Server.Data;
using JumpenoWebassembly.Shared;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Services
{
    public interface IAuthService
    {
        Task<ServiceResponse<int>> Register(User user, string password, int startUnitId);
        Task<ServiceResponse<string>> Login(string email, string password);
        Task<bool> UserExists(string email);
    }
}

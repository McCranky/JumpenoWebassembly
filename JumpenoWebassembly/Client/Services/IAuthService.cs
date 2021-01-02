using JumpenoWebassembly.Shared.Models;
using JumpenoWebassembly.Shared.Models.Request;
using JumpenoWebassembly.Shared.Models.Response;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Client.Services
{
    public interface IAuthService
    {
        Task<UserRegisterResponse> Register(UserRegisterRequest request);
        Task<UserLoginResponse> Login(UserLoginRequest request);
        Task<User> GetUser();
        Task Logout();
    }
}

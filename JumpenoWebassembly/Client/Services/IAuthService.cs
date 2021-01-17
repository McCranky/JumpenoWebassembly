using JumpenoWebassembly.Shared.Models;
using JumpenoWebassembly.Shared.Models.Request;
using JumpenoWebassembly.Shared.Models.Response;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Client.Services
{
    /// <summary>
    /// Represents service used to authentication stuff
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Register new user based on provided model
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<UserRegisterResponse> Register(UserRegisterRequest request);
        /// <summary>
        /// Login user with given credentials
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<UserLoginResponse> Login(UserLoginRequest request);
        /// <summary>
        /// Returns logged user with all data
        /// </summary>
        /// <returns></returns>
        Task<User> GetUser();
        /// <summary>
        /// Logout user from page
        /// </summary>
        /// <returns></returns>
        Task Logout();
    }
}

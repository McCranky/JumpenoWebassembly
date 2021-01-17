using JumpenoWebassembly.Server.Data;
using JumpenoWebassembly.Shared.Models;
using JumpenoWebassembly.Shared.Models.Response;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Register user and return its id.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="startSkinId"></param>
        /// <returns></returns>
        Task<ServiceResponse<long>> Register(User user, string password);
        /// <summary>
        /// Login user with given email and password.
        /// Return jwt token.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<ServiceResponse<User>> Login(string email, string password);
        /// <summary>
        /// Checks if user with given email exists.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<User> GetUser(string email);
    }
}

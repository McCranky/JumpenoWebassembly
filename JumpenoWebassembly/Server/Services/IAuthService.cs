using JumpenoWebassembly.Shared.Models;
using JumpenoWebassembly.Shared.Models.Response;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Registruje pouzivatela a vrati pridelene id.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="startSkinId"></param>
        /// <returns></returns>
        Task<ServiceResponse<long>> Register(User user, string password);

        /// <summary>
        /// Prihlasi pouzivatela s danym emailom a heslom.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<ServiceResponse<User>> Login(string email, string password);

        /// <summary>
        /// Skontroluje ci existuje pouzivatel s danym mailom.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<User> GetUser(string email);
    }
}

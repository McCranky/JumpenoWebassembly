using JumpenoWebassembly.Shared.Models;
using JumpenoWebassembly.Shared.Models.Request;
using JumpenoWebassembly.Shared.Models.Response;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Client.Services
{
    /// <summary>
    /// Reprezentuje servisu pre autentifikaciu
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registruje noveho pouzivatela na zaklade daneho modelu
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<UserRegisterResponse> Register(UserRegisterRequest request);

        /// <summary>
        /// Prihlasi pouzivatela s danymi udajmi
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<UserLoginResponse> Login(UserLoginRequest request);

        /// <summary>
        /// Vrati aktualne prihlaseneho pouzivatela so vsetkymi jeho udajmi
        /// </summary>
        /// <returns></returns>
        Task<User> GetUser();

        /// <summary>
        /// Odhlasi pouzivatela zo stranky
        /// </summary>
        /// <returns></returns>
        Task Logout();
    }
}

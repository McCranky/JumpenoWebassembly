using JumpenoWebassembly.Shared.Models;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Services
{
    /// <summary>
    /// Servis pre prihlasenych pouzivatelov
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Vrati aktualne prihlaseneho pouzivatela
        /// </summary>
        /// <returns></returns>
        Task<User> GetUser();
    }
}
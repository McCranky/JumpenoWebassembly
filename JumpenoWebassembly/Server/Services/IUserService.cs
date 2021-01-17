using JumpenoWebassembly.Shared.Models;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Services
{
    /// <summary>
    /// Service for logged user
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get currently logged user
        /// </summary>
        /// <returns></returns>
        Task<User> GetUser();
    }
}
using JumpenoWebassembly.Server.Data;
using JumpenoWebassembly.Shared.Models;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Services
{
    public interface IUserService
    {
        Task<User> GetUser();
    }
}
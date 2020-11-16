using System.Threading.Tasks;

namespace JumpenoWebassembly.Shared.Models.Hub
{
    public interface IGlobalChat
    {
        Task ReceiveMessage(string user, string message);
    }
}

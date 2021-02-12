namespace JumpenoWebassembly.Shared.Models.Response
{
    /// <summary>
    /// Odpoved servera na prihlasenie
    /// </summary>
    public class UserLoginResponse
    {
        public string Message { get; set; }
        public User User { get; set; }
    }
}

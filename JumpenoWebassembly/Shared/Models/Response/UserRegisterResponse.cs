namespace JumpenoWebassembly.Shared.Models.Response
{
    /// <summary>
    /// Odpoved servera na registraciu
    /// </summary>
    public class UserRegisterResponse
    {
        public long Id { get; set; } = -1;
        public string Message { get; set; }
    }
}

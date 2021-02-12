using System.ComponentModel.DataAnnotations;

namespace JumpenoWebassembly.Shared.Models.Request
{
    /// <summary>
    /// Model pre prihlasenie pouzivatela
    /// </summary>
    public class UserLoginRequest
    {
        [Required, EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}

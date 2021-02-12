using System;
using System.ComponentModel.DataAnnotations;

namespace JumpenoWebassembly.Shared.Models.Request
{
    /// <summary>
    /// Model pre registraciu pouzivatela
    /// </summary>
    public class UserRegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [StringLength(16, ErrorMessage = "Username is too long (16 characters max).")]
        public string Username { get; set; }
        [Required, StringLength(99, MinimumLength = 6)]
        public string Password { get; set; }
        [Required, Compare("Password", ErrorMessage = "Passwords have to match.")]
        public string ConfirmPassword { get; set; }
        public string StartSkinId { get; set; } = "1";
        [Range(typeof(bool), "true", "true", ErrorMessage = "Only confirmed users can play.")]
        public bool IsConfirmed { get; set; } = true;
    }
}

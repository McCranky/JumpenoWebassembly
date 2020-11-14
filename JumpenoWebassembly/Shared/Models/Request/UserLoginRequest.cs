using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JumpenoWebassembly.Shared.Models.Request
{
    public class UserLoginRequest
    {
        [Required, EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}

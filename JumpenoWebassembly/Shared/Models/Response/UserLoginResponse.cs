using System;
using System.Collections.Generic;
using System.Text;

namespace JumpenoWebassembly.Shared.Models.Response
{
    public class UserLoginResponse
    {
        public string Message { get; set; }
        public User User { get; set; }
    }
}

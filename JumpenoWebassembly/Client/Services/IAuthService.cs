using JumpenoWebassembly.Shared.Models.Request;
using JumpenoWebassembly.Shared.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Client.Services
{
    interface IAuthService
    {
        Task<UserRegisterResponse> Register(UserRegisterRequest request);
        Task<UserLoginResponse> Login(UserLoginRequest request);
    }
}

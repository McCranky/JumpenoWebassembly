using JumpenoWebassembly.Server.Data;
using JumpenoWebassembly.Server.Options;
using JumpenoWebassembly.Shared.Models;
using JumpenoWebassembly.Shared.Models.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;

        public AuthService(DataContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Login user with given email and password.
        /// Return jwt token.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<ServiceResponse<User>> Login(string email, string password)
        {
            var response = new ServiceResponse<User> { Success = false };
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Email.ToLower() == email.ToLower());

            if (user == null) {
                response.Message = "User doesn't exist.";
            } else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) {
                response.Message = "Incorrect password.";
            } else {
                response.Success = true;
                response.Message = "Login successful.";
                response.Data = user;
            }

            return response;
        }

        public async Task<ServiceResponse<long>> Register(User user, string password, int startSkinId)
        {
            if (await GetUser(user.Email) != null) {
                return new ServiceResponse<long> {
                    Success = false,
                    Message = "User with this email already exists."
                };
            }

            CreatePasswordHash(password, out var passwordHash, out var passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            //TODO add starting skin

            return new ServiceResponse<long> {
                Data = user.Id,
                Success = true,
                Message = "Registration successful."
            };
        }

        public async Task<User> GetUser(string email)
        {
            var user = await _context.Users.Where(user => user.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
            
            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            for (int i = 0; i < computedHash.Length; i++) {
                if (computedHash[i] != passwordHash[i]) {
                    return false;
                }
            }
            return true;
        }
    }
}

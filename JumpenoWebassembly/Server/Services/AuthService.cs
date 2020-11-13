using JumpenoWebassembly.Server.Data;
using JumpenoWebassembly.Server.Options;
using JumpenoWebassembly.Shared;
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
        private readonly JwtSettings _jwtSettings;

        public AuthService(DataContext context, JwtSettings jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings;
        }

        /// <summary>
        /// Login user with given email and password.
        /// Return jwt token.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<ServiceResponse<string>> Login(string email, string password)
        {
            var response = new ServiceResponse<string> { Success = false };
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Email.ToLower() == email.ToLower());
            if (user == null) {
                response.Message = "User doesn't exist.";
            } else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) {
                response.Message = "Incorrect password.";
            } else {
                response.Success = true;
                response.Message = "Login successful.";
                response.Data = CreateToken(user);
            }

            return response;
        }

        /// <summary>
        /// Register user and return its id.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="startUnitId"></param>
        /// <returns></returns>
        public async Task<ServiceResponse<int>> Register(User user, string password, int startUnitId)
        {
            if (await UserExists(user.Email)) {
                return new ServiceResponse<int> {
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
            return new ServiceResponse<int> {
                Data = user.Id,
                Success = true,
                Message = "Registration successful."
            };
        }

        /// <summary>
        /// Checks if user with given email exists.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> UserExists(string email)
        {
            if (await _context.Users.AnyAsync(user => user.Email.ToLower() == email.ToLower())) {
                return true;
            }
            return false;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512()) {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)) {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                for (int i = 0; i < computedHash.Length; i++) {
                    if (computedHash[i] != passwordHash[i]) {
                        return false;
                    }
                }
                return true;
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
                );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }

        private async Task AddStartingSkin(User user, int skinId)
        {
            //var unit = await _context.Units.FirstOrDefaultAsync<Unit>(u => u.Id == startUnitId);
            //await _context.UserUnits.AddAsync(new UserUnit {
            //    UnitId = unit.Id,
            //    UserId = user.Id,
            //    HitPoints = unit.HitPoints
            //});

            await _context.SaveChangesAsync();
        }
    }
}

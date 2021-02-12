using JumpenoWebassembly.Server.Data;
using JumpenoWebassembly.Shared.Models;
using JumpenoWebassembly.Shared.Models.Response;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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

        public async Task<ServiceResponse<long>> Register(User user, string password)
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

            if (user.Id == default) {
                var count = _context.Users.Count();
                if (count > 0) {
                    var intIds = _context.Users.Select(usr => usr.Id).Where(id => id < int.MaxValue);
                    if (intIds.Count() > 0) {
                        user.Id = intIds.Max() + 1;
                    } else {
                        user.Id = 11;
                    }
                } else {
                    user.Id = 11;
                }
            }

            user.Skin = "mageSprite_magic";

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

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

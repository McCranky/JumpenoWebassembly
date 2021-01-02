using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Shared.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public bool IsConfirmed { get; set; }
        public bool IsDeleted { get; set; }
        public int SkinId { get; set; }
        public int TotalScore { get; set; }
        public int GamesPlayed { get; set; }
        public int Victories { get; set; }
    }
}

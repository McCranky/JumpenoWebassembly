using JumpenoWebassembly.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace JumpenoWebassembly.Server.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}

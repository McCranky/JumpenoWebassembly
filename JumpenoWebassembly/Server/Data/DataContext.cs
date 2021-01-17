using JumpenoWebassembly.Shared.Jumpeno.Utilities;
using JumpenoWebassembly.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace JumpenoWebassembly.Server.Data
{
    /// <summary>
    /// Database for Jumpeno.
    /// Contains user and map tables
    /// </summary>
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<MapTemplate> Maps { get; set; }
    }
}

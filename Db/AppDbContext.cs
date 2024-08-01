using Microsoft.EntityFrameworkCore;
using Parser.Models;

namespace Parser.Db
{
    public class AppDbContext : DbContext
    {
        public DbSet<Purchase> Purchases { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}

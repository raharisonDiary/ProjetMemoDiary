using Microsoft.EntityFrameworkCore;
using SocialGasy.Models;

namespace SocialGasy.Data
{
    public class SocialGasyDbContext : DbContext
    {
        public SocialGasyDbContext(DbContextOptions<SocialGasyDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Citizen> Citizens { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        
        // Nampiana ity line ity:
        public DbSet<Household> Households { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Citizen>()
                .HasIndex(c => c.CIN)
                .IsUnique();
        }
    }
}
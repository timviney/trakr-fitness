using Microsoft.EntityFrameworkCore;
using GymTracker.Core.Entities;

namespace GymTracker.Infrastructure.Data
{
    public class GymTrackerDbContext(DbContextOptions<GymTrackerDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);
                b.Property(u => u.Username).IsRequired();
                b.HasIndex(u => u.Username).IsUnique();
                b.Property(u => u.Username).HasMaxLength(50);
            });
        }
    }
}

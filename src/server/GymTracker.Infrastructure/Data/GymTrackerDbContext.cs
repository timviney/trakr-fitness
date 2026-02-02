using Microsoft.EntityFrameworkCore;
using GymTracker.Core.Entities;

namespace GymTracker.Infrastructure.Data
{
    public class GymTrackerDbContext(DbContextOptions<GymTrackerDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<MuscleCategory> MuscleCategories { get; set; } = null!;
        public DbSet<MuscleGroup> MuscleGroups { get; set; } = null!;
        public DbSet<Exercise> Exercises { get; set; } = null!;
        public DbSet<Workout> Workouts { get; set; } = null!;
        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<SessionExercise> SessionExercises { get; set; } = null!;
        public DbSet<Set> Sets { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);
                b.Property(u => u.Email).IsRequired();
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.Email).HasMaxLength(50);
                b.HasMany(u => u.Workouts)
                    .WithOne(w => w.User)
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasMany(u => u.Exercises)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasMany(u => u.MuscleGroups)
                    .WithOne(mg => mg.User)
                    .HasForeignKey(mg => mg.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasMany(u => u.MuscleCategories)
                    .WithOne(mc => mc.User)
                    .HasForeignKey(mc => mc.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // MuscleCategory configuration
            modelBuilder.Entity<MuscleCategory>(b =>
            {
                b.HasKey(mc => mc.Id);
                b.Property(mc => mc.Name).IsRequired();
            });

            // MuscleGroup configuration
            modelBuilder.Entity<MuscleGroup>(b =>
            {
                b.HasKey(mg => mg.Id);
                b.Property(mg => mg.Name).IsRequired();
                b.HasOne(mg => mg.Category)
                    .WithMany(mc => mc.MuscleGroups)
                    .HasForeignKey(mg => mg.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Exercise configuration
            modelBuilder.Entity<Exercise>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Name).IsRequired();
                b.HasOne(e => e.MuscleGroup)
                    .WithMany(mg => mg.Exercises)
                    .HasForeignKey(e => e.MuscleGroupId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Workout configuration
            modelBuilder.Entity<Workout>(b =>
            {
                b.HasKey(w => w.Id);
                b.Property(w => w.Name).IsRequired();
                b.HasMany(w => w.Sessions)
                    .WithOne(s => s.Workout)
                    .HasForeignKey(s => s.WorkoutId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Session configuration
            modelBuilder.Entity<Session>(b =>
            {
                b.HasKey(s => s.Id);
                b.HasMany(s => s.SessionExercises)
                    .WithOne(se => se.Session)
                    .HasForeignKey(se => se.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SessionExercise configuration
            modelBuilder.Entity<SessionExercise>(b =>
            {
                b.HasKey(se => se.Id);
                b.HasOne(se => se.Exercise)
                    .WithMany(e => e.SessionExercises)
                    .HasForeignKey(se => se.ExerciseId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasMany(se => se.Sets)
                    .WithOne(s => s.SessionExercise)
                    .HasForeignKey(s => s.SessionExerciseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Set configuration
            modelBuilder.Entity<Set>(b =>
            {
                b.HasKey(s => s.Id);
            });
        }
    }
}

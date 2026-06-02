using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class JobLockDbContext(DbContextOptions<JobLockDbContext> options) : DbContext(options)
{
    public DbSet<JobLock> JobLocks => Set<JobLock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobLock>()
            .ToContainer("JobSnapshots")
            .HasPartitionKey(l => l.Region)
            .HasNoDiscriminator()
            .UseETagConcurrency()  // Crucial for UC 2.1 Locking
            .HasKey(l => l.Id);

        modelBuilder.Entity<JobLock>()
            .Property(l => l.Id).HasConversion<string>();

        modelBuilder.Entity<JobLock>()
            .Property(l => l.Id).ToJsonProperty("id");

        modelBuilder.Entity<JobLock>()
            .Property(l => l.Region).ToJsonProperty("region");

        modelBuilder.Entity<JobLock>()
            .Property(j => j.IsLocked).ToJsonProperty("is_locked");

        modelBuilder.Entity<JobLock>()
            .Property(j => j.LockedAt).ToJsonProperty("locked_at");
    }
}
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class JobLockDbContext(DbContextOptions<JobLockDbContext> options) : DbContext(options)
{
    public DbSet<JobLock> JobLocks => Set<JobLock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var jobLock = modelBuilder.Entity<JobLock>();

        jobLock.ToContainer("JobSnapshots")
            .HasPartitionKey(l => l.JobId)
            .HasNoDiscriminator()
            .HasKey(l => l.Id);

        jobLock.Property(l => l.Id)
            .ToJsonProperty("id");

        jobLock.Property(l => l.JobId)
            .ToJsonProperty("job_id");

        jobLock.Property(j => j.LockedAt)
            .ToJsonProperty("locked_at");

        jobLock.Property(e => e.TimeToLive)
            .ToJsonProperty("ttl");
    }
}
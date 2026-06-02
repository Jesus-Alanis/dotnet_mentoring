using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class JobReadDbContext(DbContextOptions<JobReadDbContext> options) : DbContext(options)
{
    public DbSet<JobRecord> Jobs => Set<JobRecord>();
    public DbSet<JobExecution> JobExecutions => Set<JobExecution>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Enforce No-Tracking globally for the read replica to boost performance
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobExecution>()
            .ToTable("JobExecution").HasKey(j => j.Id);

        modelBuilder.Entity<JobExecution>()
            .Property(j => j.JobId).HasColumnName("job_id");

        modelBuilder.Entity<JobExecution>()
            .Property(j => j.StartedAt).HasColumnName("started_at");

        modelBuilder.Entity<JobExecution>()
            .Property(j => j.EndedAt).HasColumnName("ended_at");

        modelBuilder.Entity<JobExecution>()
           .Property(j => j.Status).HasConversion<string>();

        modelBuilder.Entity<JobExecution>()
            .HasOne(e => e.Job)
            .WithMany(j => j.Executions)
            .HasForeignKey(e => e.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JobExecution>()
            .HasIndex(e => new { e.JobId, e.Status });

        modelBuilder.Entity<JobRecord>()
            .ToTable("Job").HasKey(j => j.Id);

        modelBuilder.Entity<JobRecord>()
            .Property(j => j.OwnerId).HasColumnName("user_id");

        modelBuilder.Entity<JobRecord>()
           .Property(j => j.CronExpression).HasColumnName("frequency");

        modelBuilder.Entity<JobRecord>()
          .Property(j => j.CreatedAt).HasColumnName("created_at");

        modelBuilder.Entity<JobRecord>()
           .Property(j => j.Status).HasConversion<string>();
    }
}
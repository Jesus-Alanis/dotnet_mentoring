using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class JobSqlDbContext(DbContextOptions<JobSqlDbContext> options) : DbContext(options)
{
    public DbSet<JobRecord> Jobs => Set<JobRecord>();
    public DbSet<JobExecution> JobExecutions => Set<JobExecution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var jobExecution = modelBuilder.Entity<JobExecution>();

        jobExecution.ToTable("JobExecution").HasKey(j => new { j.Id, j.ScheduledTime });

        jobExecution.Property(j => j.JobId)
            .HasColumnName("job_id");

        jobExecution.Property(j => j.ScheduledTime)
            .HasColumnName("scheduled_time");

        jobExecution.Property(j => j.StartedAt)
            .HasColumnName("started_at");

        jobExecution.Property(j => j.EndedAt)
            .HasColumnName("ended_at");

        jobExecution.Property(j => j.Status)
            .HasConversion<string>();

        jobExecution.Property(j => j.ErrorMessage)
            .HasColumnName("error_message");

        jobExecution
            .HasOne(e => e.Job)
            .WithMany(j => j.Executions)
            .HasForeignKey(e => e.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        jobExecution.HasIndex(e => new { e.JobId, e.Status });


        var jobRecord = modelBuilder.Entity<JobRecord>();

        jobRecord.ToTable("Job").HasKey(j => j.Id);

        jobRecord.Property(j => j.OwnerId)
            .HasColumnName("user_id");

        jobRecord.Property(j => j.CronExpression)
            .HasColumnName("frequency");

        jobRecord.Property(j => j.IsRecurrent)
            .HasColumnName("is_recurrent");

        jobRecord.Property(j => j.ScheduledTime)
            .HasColumnName("next_execution_at");

        jobRecord.Property(j => j.CreatedAt)
            .HasColumnName("created_at");

        jobRecord.Property(j => j.Status)
            .HasConversion<string>();

        jobRecord.HasIndex(e => new { e.Id, e.Status });
    }
}
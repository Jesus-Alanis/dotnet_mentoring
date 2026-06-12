using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Data.Repositories;

public interface IJobStoreRepository
{
    Task<List<JobRecord>> GetJobsByOwnerId(Guid ownerId, DateTimeOffset startDate, DateTimeOffset endDate, JobStatus? jobStatus, CancellationToken ct);
    Task<DateTimeOffset> GetJobScheduledTimeAsync(Guid jobId, CancellationToken ct);

    Task<JobRecord?> GetJobByIdAsync(Guid jobId, CancellationToken ct);
    Task<Guid> CreateJobAsync(JobRecord job, CancellationToken ct);
    Task UpdateJobAsync(JobRecord job, CancellationToken ct);

    Task<JobExecution?> GetJobExecutionByIdAsync(Guid jobId, DateTimeOffset scheduledTime, CancellationToken ct);
    Task<List<JobExecution>> GetJobExecutionHistoryAsync(Guid jobId, DateTimeOffset startDate, DateTimeOffset endDate, JobExecutionStatus? jobExecutionStatus, CancellationToken ct);
    Task CreateJobExecutionAsync(JobExecution jobExecution, CancellationToken ct);
    Task UpdateJobExecutionAsync(JobExecution jobExecution, CancellationToken ct);    
}

public class JobStoreRepository(JobStoreDbContext dbContext) : IJobStoreRepository
{
    public async Task<List<JobRecord>> GetJobsByOwnerId(Guid ownerId, DateTimeOffset startDate, DateTimeOffset endDate, JobStatus? jobStatus, CancellationToken ct)
    {
        return await dbContext.Jobs
            .Where(j => j.OwnerId == ownerId
                    && j.ScheduledTime >= startDate
                    && j.ScheduledTime <= endDate
                    && (!jobStatus.HasValue || j.Status == jobStatus.Value))
            .ToListAsync(ct);
    }

    public async Task<DateTimeOffset> GetJobScheduledTimeAsync(Guid jobId, CancellationToken ct)
    {
        return await dbContext.Jobs
            .Where(j => j.Id == jobId)
            .Select(j => j.ScheduledTime)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<JobRecord?> GetJobByIdAsync(Guid jobId, CancellationToken ct)
       => await dbContext.Jobs.FindAsync([jobId], ct);

    public async Task<Guid> CreateJobAsync(JobRecord job, CancellationToken ct)
    {        
        dbContext.Jobs.Add(job);
        await dbContext.SaveChangesAsync(ct);

        return job.Id;
    }

    public async Task UpdateJobAsync(JobRecord job, CancellationToken ct)
    {
        dbContext.Jobs.Update(job);

        // Execute using the architecture's recommended READ COMMITTED isolation level
        // to prevent dirty reads while the job is starting up.
        using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task<JobExecution?> GetJobExecutionByIdAsync(Guid jobId, DateTimeOffset scheduledTime, CancellationToken ct)
        => await dbContext.JobExecutions.FindAsync([jobId, scheduledTime], ct);

    public async Task<List<JobExecution>> GetJobExecutionHistoryAsync(Guid jobId, DateTimeOffset startDate, DateTimeOffset endDate, JobExecutionStatus? jobExecutionStatus, CancellationToken ct)
    {
        return await dbContext.JobExecutions
            .Where(j => j.JobId == jobId
                    && j.ScheduledTime >= startDate
                    && j.ScheduledTime <= endDate
                    && (!jobExecutionStatus.HasValue || j.Status == jobExecutionStatus.Value))
            .ToListAsync(ct);
    }

    public async Task CreateJobExecutionAsync(JobExecution jobExecution, CancellationToken ct)
    {
        dbContext.JobExecutions.Add(jobExecution);

        // Execute using the architecture's recommended READ COMMITTED isolation level
        // to prevent dirty reads while the job is starting up.
        using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task UpdateJobExecutionAsync(JobExecution jobExecution, CancellationToken ct)
    {
        dbContext.JobExecutions.Update(jobExecution);

        // Execute using the architecture's recommended READ COMMITTED isolation level
        // to prevent dirty reads while the job is starting up.
        using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
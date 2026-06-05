using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Application.Commands;

public record StartJobExecutionCommand(Guid JobId, DateTimeOffset ScheduledTime, string WorkerNodeId);
public record CompleteJobExecutionCommand(Guid JobExecutionId, DateTimeOffset ScheduledTime, JobExecutionStatus FinalStatus, string? ErrorMessage = null);

public class JobExecutionCommandHandler(JobSqlDbContext sqlDbContext)
{
    public async Task<Guid> HandleStartAsync(StartJobExecutionCommand request, CancellationToken ct)
    {
        var execution = new JobExecution
        {
            Id = Guid.NewGuid(),
            JobId = request.JobId,
            WorkerNodeId = request.WorkerNodeId,
            Status = JobExecutionStatus.Running,
            ScheduledTime = request.ScheduledTime,
            StartedAt = DateTimeOffset.UtcNow
        };

        sqlDbContext.JobExecutions.Add(execution);

        // Execute using the architecture's recommended READ COMMITTED isolation level
        // to prevent dirty reads while the job is starting up.
        using var transaction = await sqlDbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

        await sqlDbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return execution.Id;
    }

    public async Task HandleCompletionAsync(CompleteJobExecutionCommand request, CancellationToken ct)
    {
        var execution = await sqlDbContext.JobExecutions.FindAsync([request.JobExecutionId, request.ScheduledTime], ct);

        if (execution != null)
        {
            execution.Status = request.FinalStatus;
            execution.EndedAt = DateTimeOffset.UtcNow;

            if(!string.IsNullOrEmpty(request.ErrorMessage))
                execution.ErrorMessage = request.ErrorMessage;

            using var transaction = await sqlDbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            await sqlDbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
    }
}

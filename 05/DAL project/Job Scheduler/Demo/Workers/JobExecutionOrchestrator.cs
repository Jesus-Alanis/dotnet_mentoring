using Application.Commands;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Demo.Workers;

public class JobExecutionOrchestrator(
    AcquireJobLockCommandHandler lockHandler,
    ReleaseJobLockCommandHandler releaseHandler,
    JobExecutionCommandHandler executionHandler,
    ILogger<JobExecutionOrchestrator> logger)
{
    public async Task ProcessJobAsync(Guid jobId, string workerId, string region, CancellationToken ct)
    {
        // Acquire Lock in Cosmos DB (Optimistic Concurrency Control)
        var lockCommand = new AcquireJobLockCommand(jobId, workerId, region);
        bool hasLock = await lockHandler.HandleAsync(lockCommand, ct);

        if (!hasLock)
        {
            logger.LogInformation("Worker ID: {workerId} - Job {JobId} is locked.", workerId, jobId);
            return;
        }

        logger.LogInformation("Worker ID: {workerId} - Job {JobId} running.", workerId, jobId);
        var startCommand = new StartJobExecutionCommand(jobId, workerId);
        var jobExecutionId = await executionHandler.HandleStartAsync(startCommand, ct);

        try
        {          
            await PerformHeavyWorkloadAsync(ct);

            await executionHandler.HandleCompletionAsync(
                new CompleteJobExecutionCommand(jobExecutionId, JobExecutionStatus.Completed), ct);
            logger.LogInformation("Worker ID: {workerId} - Job {JobId} completed.", workerId, jobId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Worker ID: {workerId} - Job {JobId} failed.", workerId, jobId);

            await executionHandler.HandleCompletionAsync(
                new CompleteJobExecutionCommand(jobExecutionId, JobExecutionStatus.Failed), ct);
        }

        await releaseHandler.HandleAsync(new ReleaseJobLockCommand(jobId, region), ct);
    }

    private Task PerformHeavyWorkloadAsync(CancellationToken ct) => Task.Delay(2000, ct);
}

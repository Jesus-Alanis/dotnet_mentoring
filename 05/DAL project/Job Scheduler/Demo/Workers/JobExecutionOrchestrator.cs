using Application.Commands;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Demo.Workers;

public class JobExecutionOrchestrator(
    AcquireJobLockCommandHandler lockHandler,
    ReleaseJobLockCommandHandler releaseHandler,
    JobExecutionCommandHandler executionHandler,
    UpdateJobScheduledTimeCommandHandler updateJobHandler,
    ILogger<JobExecutionOrchestrator> logger)
{

    private readonly string WorkerId = $"Worker_{Guid.NewGuid().ToString()}";

    public async Task ProcessJobAsync(Guid JobId, DateTimeOffset JobScheduledTime, CancellationToken ct)
    {
        // Acquire Lock in Cosmos DB (Optimistic Concurrency Control)
        var lockCommand = new AcquireJobLockCommand(JobId, WorkerId);
        bool hasLock = await lockHandler.HandleAsync(lockCommand, ct);

        if (!hasLock)
        {
            logger.LogInformation("Worker ID: {workerId} - Job: {JobId} is locked.", WorkerId, JobId);
            return;
        }

        logger.LogInformation("Worker ID: {workerId} - Job: {JobId} running.", WorkerId, JobId);
        var startCommand = new StartJobExecutionCommand(JobId, JobScheduledTime, WorkerId);
        var jobExecutionId = await executionHandler.HandleStartAsync(startCommand, ct);

        try
        {          
            await PerformHeavyWorkloadAsync(ct);

            await executionHandler.HandleCompletionAsync(
                new CompleteJobExecutionCommand(jobExecutionId, JobScheduledTime, JobExecutionStatus.Completed), ct);
            logger.LogInformation("Worker ID: {workerId} - Job: {JobId} completed.", WorkerId, JobId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Worker ID: {workerId} - Job: {JobId} failed.", WorkerId, JobId);

            await executionHandler.HandleCompletionAsync(
                new CompleteJobExecutionCommand(jobExecutionId, JobScheduledTime, JobExecutionStatus.Failed, ex.Message), ct);
        }
        finally
        {
            await updateJobHandler.HandleAsync(new UpdateJobScheduledTimeCommand(JobId), ct);
            await releaseHandler.HandleAsync(new ReleaseJobLockCommand(JobId), ct);
        }       
    }

    private Task PerformHeavyWorkloadAsync(CancellationToken ct) => Task.Delay(2000, ct);
}

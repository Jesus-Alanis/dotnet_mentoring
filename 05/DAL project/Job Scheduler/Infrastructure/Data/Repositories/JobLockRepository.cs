using Domain.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Repositories;

public interface IJobLockRepository
{
    Task<bool> TryAcquireJobLockAsync(JobLock jobLock, CancellationToken ct);
    Task<bool> ReleaseJobLockAsync(Guid jobId, Domain.Enums.ConsistencyLevel consistencyLevel, CancellationToken ct);
}

public class JobLockRepository : IJobLockRepository
{
    private readonly Container _lockContainer;
    private readonly ILogger<JobLockRepository> _logger;

    public JobLockRepository(JobLockDbContext cosmosDbContext, ILogger<JobLockRepository> logger)
    {
        _lockContainer = cosmosDbContext.Database.GetCosmosClient()
                                .GetContainer(databaseId: "JobScheduler", containerId: "JobSnapshots");
        _logger = logger;
    }

    public async Task<bool> TryAcquireJobLockAsync(JobLock jobLock, CancellationToken ct)
    {
        // Job execution ALWAYS requires Strong Consistency 
        // to ensure no double-execution
        var options = GetRequestOptions(Domain.Enums.ConsistencyLevel.Strong);

        var lockDoc = new
        {
            id = JobLock.IdFormat(jobLock.JobId),
            job_id = jobLock.JobId.ToString(),
            locked_by_worker_id = jobLock.LockedByWorkerId,
            locked_at = jobLock.LockedAt,
            ttl = jobLock.TimeToLive
        };

        try
        {
            var response = await _lockContainer.CreateItemAsync(lockDoc,
                new PartitionKey(jobLock.JobId.ToString()),
                requestOptions: options,
                ct);

            return response.StatusCode == System.Net.HttpStatusCode.Created;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            return false; // 409 Conflict - Someone else grabbed the lock
        }
    }

    public async Task<bool> ReleaseJobLockAsync(Guid jobId, Domain.Enums.ConsistencyLevel consistencyLevel, CancellationToken ct)
    {
        var requestOptions = GetRequestOptions(consistencyLevel);

        try
        {
            var response = await _lockContainer.DeleteItemAsync<JobLock>(
                id: JobLock.IdFormat(jobId),
                partitionKey: new PartitionKey(jobId.ToString()),
                requestOptions: requestOptions,
                ct);

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private ItemRequestOptions GetRequestOptions(Domain.Enums.ConsistencyLevel consistencyLevel)
    {
        var options = new ItemRequestOptions
        {
            // Map Domain Consistency -> Native Cosmos DB Consistency
            ConsistencyLevel = consistencyLevel switch
            {
                Domain.Enums.ConsistencyLevel.Strong => Microsoft.Azure.Cosmos.ConsistencyLevel.Strong,
                Domain.Enums.ConsistencyLevel.Eventual => Microsoft.Azure.Cosmos.ConsistencyLevel.Eventual,
                // Cosmos DB's 'Session' is its native mechanism for Read-After-Write
                Domain.Enums.ConsistencyLevel.ReadAfterWrite => Microsoft.Azure.Cosmos.ConsistencyLevel.Session,
                _ => null // Defers to account default
            }
        };

        _logger.LogInformation(">>> COSMOS ROUTING DECISION: Utilizing '{NativeConsistency}' consistency token <<<",
                options.ConsistencyLevel?.ToString() ?? "Account Default");

        return options;
    }
}
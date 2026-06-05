using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands;

public record AcquireJobLockCommand(Guid JobId, string WorkerId);

public class AcquireJobLockCommandHandler(JobLockDbContext cosmosDbContext)
{
    public async Task<bool> HandleAsync(AcquireJobLockCommand request, CancellationToken ct)
    {
        // Read the lock document (Strong Consistency Quorum Read)
        var lockDoc = await cosmosDbContext.JobLocks
            .WithPartitionKey(request.JobId)
            .FirstOrDefaultAsync(l => l.Id == JobLock.IdFormat(request.JobId), ct);

        if (lockDoc is not null)
        {
            return false;
        }

        try
        {
            lockDoc = new JobLock
            {
                Id = JobLock.IdFormat(request.JobId),
                JobId = request.JobId,
                LockedByWorkerId = request.WorkerId,
                LockedAt = DateTimeOffset.UtcNow
            };

            cosmosDbContext.JobLocks.Add(lockDoc);
            await cosmosDbContext.SaveChangesAsync(ct);

            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("Conflict") == true)
        {
            //If document is new, Cosmos DB strictly enforces Unique ID constraints within a partition, rejecting consecutive inserts for the same lock.
            return false;
        }
    }
}
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands;

public record AcquireJobLockCommand(Guid JobId, string WorkerId, string Region);

public class AcquireJobLockCommandHandler(JobLockDbContext cosmosDbContext)
{
    public async Task<bool> HandleAsync(AcquireJobLockCommand request, CancellationToken ct)
    {
        // Read the lock document (Strong Consistency Quorum Read)
        var lockDoc = await cosmosDbContext.JobLocks
            .WithPartitionKey(request.Region)
            .FirstOrDefaultAsync(l => l.Id == request.JobId, ct);

        if (lockDoc == null)
        {
            lockDoc = new JobLock
            {
                Id = request.JobId,
                Region = request.Region,
                LockedByWorkerId = request.WorkerId
            };

            cosmosDbContext.JobLocks.Add(lockDoc);
        }

        if (lockDoc.IsLocked)
        {
           return false; // Already locked
        }

        // Attempt to lock
        lockDoc.IsLocked = true;
        lockDoc.LockedAt = DateTimeOffset.UtcNow;

        try
        {
            // EF Core automatically injects "If-Match: {etag}" in the REST API call            
            await cosmosDbContext.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Failure: Another worker updated the document first, and the ETag mismatched.
            return false;
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("Conflict") == true)
        {
            //If document is new, Cosmos DB strictly enforces Unique ID constraints within a partition, rejecting consecutive inserts for the same lock.
            return false;
        }
    }
}
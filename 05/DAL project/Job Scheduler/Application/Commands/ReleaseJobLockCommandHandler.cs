using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands;

public record ReleaseJobLockCommand(Guid JobId);

public class ReleaseJobLockCommandHandler(JobLockDbContext cosmosDbContext)
{
    public async Task<bool> HandleAsync(ReleaseJobLockCommand request, CancellationToken ct)
    {     
        try
        {
            var jobLock = await cosmosDbContext.JobLocks
                .WithPartitionKey(request.JobId)
                .Where(l => l.Id == JobLock.IdFormat(request.JobId))
                .FirstOrDefaultAsync(ct);

            if (jobLock == null)
                return true;

            cosmosDbContext.JobLocks.Remove(jobLock);
            await cosmosDbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
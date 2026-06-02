using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Commands;

public record ReleaseJobLockCommand(Guid JobId, string Region);

public class ReleaseJobLockCommandHandler(JobLockDbContext cosmosDbContext)
{
    public async Task<bool> HandleAsync(ReleaseJobLockCommand request, CancellationToken ct)
    {     
        try
        {
            var deleted = await cosmosDbContext.JobLocks
                .WithPartitionKey(request.Region)
                .Where(l => l.Id == request.JobId)
                .ExecuteDeleteAsync(ct);
            return deleted > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
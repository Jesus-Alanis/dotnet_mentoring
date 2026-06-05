using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries;

public record GetJobScheduledTimeQuery(Guid JobId);

public class GetJobScheduledTimeQueryHandler(JobReadDbContext readDbContext)
{
    public async Task<DateTimeOffset> HandleAsync(GetJobScheduledTimeQuery request, CancellationToken ct)
    {
        return await readDbContext.Jobs
            .Where(j => j.Id == request.JobId && j.Status == JobStatus.Active)
            .Select(j => j.ScheduledTime)
            .FirstOrDefaultAsync(ct);
    }
}
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Queries;

public record GetActiveJobsQuery(Guid OwnerId);

public class GetActiveJobsQueryHandler(JobReadDbContext readDbContext)
{
    public async Task<List<JobRecord>> HandleAsync(GetActiveJobsQuery request, CancellationToken ct)
    {
        return await readDbContext.Jobs
            .Where(j => j.OwnerId == request.OwnerId && j.Status == JobStatus.Active)
            .ToListAsync(ct);
    }
}
using Application.Extensions;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Application.Commands;

public record UpdateJobScheduledTimeCommand(Guid JobId);

public class UpdateJobScheduledTimeCommandHandler(JobSqlDbContext writeDbContext)
{
    public async Task<bool> HandleAsync(UpdateJobScheduledTimeCommand request, CancellationToken ct)
    {
        var job = await writeDbContext.Jobs
            .Where(j => j.Id == request.JobId && j.Status == JobStatus.Active)
            .FirstOrDefaultAsync(ct);

        if (job == null)
            return false;

        var nextOCcurrence = job.CronExpression.GetNextExecution(afterTime: job.ScheduledTime);
        if (job.IsRecurrent
            && nextOCcurrence.HasValue)
        {
            job.ScheduledTime = nextOCcurrence.Value;

            await writeDbContext.SaveChangesAsync(ct);
        }       

        return true;
    }
}
using Domain.Entities;
using Application.Extensions;
using Infrastructure.Data;

namespace Application.Commands;

public record CreateJobCommand(Guid OwnerId, string Name, string CronExpression, bool isRecurrent, string Payload);

public class CreateJobCommandHandler(JobSqlDbContext writeDbContext)
{
    public async Task<Guid> HandleAsync(CreateJobCommand request, CancellationToken ct)
    {
        //ScheduledTime cannot be null at the time of job creation, it needs to run at least once.
        var nextOCcurrence = request.CronExpression.GetNextExecution() 
            ?? DateTimeOffset.UtcNow.AddMinutes(5);

        var job = new JobRecord
        {
            Id = Guid.NewGuid(),
            OwnerId = request.OwnerId,
            Name = request.Name,
            CronExpression = request.CronExpression,
            Payload = request.Payload,
            IsRecurrent = request.isRecurrent,
            ScheduledTime = nextOCcurrence
        };

        writeDbContext.Jobs.Add(job);
        await writeDbContext.SaveChangesAsync(ct);

        return job.Id;
    }
}
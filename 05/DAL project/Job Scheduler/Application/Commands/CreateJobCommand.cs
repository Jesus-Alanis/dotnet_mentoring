using Domain.Entities;
using Infrastructure.Data;

namespace Application.Commands;

public record CreateJobCommand(Guid OwnerId, string Name, string CronExpression, string Payload);

public class CreateJobCommandHandler(JobSqlDbContext writeDbContext)
{
    public async Task<Guid> HandleAsync(CreateJobCommand request, CancellationToken ct)
    {
        var job = new JobRecord
        {
            Id = Guid.NewGuid(),
            OwnerId = request.OwnerId,
            Name = request.Name,
            CronExpression = request.CronExpression,
            Payload = request.Payload
        };

        writeDbContext.Jobs.Add(job);
        await writeDbContext.SaveChangesAsync(ct);

        return job.Id;
    }
}
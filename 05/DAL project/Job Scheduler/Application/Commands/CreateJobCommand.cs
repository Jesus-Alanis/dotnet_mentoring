using Application.Extensions;
using Domain.Entities;
using Infrastructure.Data;
using MediatR;

namespace Application.Commands;

public record CreateJobCommand(Guid OwnerId, string Name, string CronExpression, bool IsRecurrent, string? Payload = null)
    : IRequest<Guid>;

public class CreateJobCommandHandler(SqlConnectionFactory connectionFactory)
    : IRequestHandler<CreateJobCommand, Guid>
{
    public async Task<Guid> Handle(CreateJobCommand request, CancellationToken ct)
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
            IsRecurrent = request.IsRecurrent,
            ScheduledTime = nextOCcurrence
        };

        return await connectionFactory.GetJobStoreRepository(Domain.Enums.ConsistencyLevel.Strong, request.OwnerId)
            .CreateJobAsync(job, ct);
    }
}
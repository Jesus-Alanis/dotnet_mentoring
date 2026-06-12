using Domain.Entities;
using Infrastructure.Data;
using MediatR;

namespace Application.Commands;

public record StartJobExecutionCommand(Guid JobId, DateTimeOffset ScheduledTime, string WorkerNodeId)
    : IRequest;

public class StartJobExecutionCommandHandler(SqlConnectionFactory connectionFactory)
    : IRequestHandler<StartJobExecutionCommand>
{
    public async Task Handle(StartJobExecutionCommand request, CancellationToken ct)
    {
        var jobExecution = new JobExecution
        {
            JobId = request.JobId,
            WorkerNodeId = request.WorkerNodeId,
            Status = JobExecutionStatus.Running,
            ScheduledTime = request.ScheduledTime,
            StartedAt = DateTimeOffset.UtcNow
        };

        await connectionFactory.GetJobStoreRepository(Domain.Enums.ConsistencyLevel.Strong)
            .CreateJobExecutionAsync(jobExecution, ct);
    }
}
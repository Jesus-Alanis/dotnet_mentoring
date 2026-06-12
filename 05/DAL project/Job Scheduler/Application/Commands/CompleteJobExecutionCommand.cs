using Domain.Entities;
using Infrastructure.Data;
using MediatR;

namespace Application.Commands;

public record CompleteJobExecutionCommand(Guid JobId, DateTimeOffset ScheduledTime, JobExecutionStatus FinalStatus, string? ErrorMessage = null)
    : IRequest;

public class CompleteJobExecutionCommandHandler(SqlConnectionFactory connectionFactory)
    : IRequestHandler<CompleteJobExecutionCommand>
{
    public async Task Handle(CompleteJobExecutionCommand request, CancellationToken ct)
    {
        var writeRepository = connectionFactory.GetJobStoreRepository(Domain.Enums.ConsistencyLevel.Strong);

        var jobExecution = await writeRepository.GetJobExecutionByIdAsync(request.JobId, request.ScheduledTime, ct);

        if (jobExecution != null)
        {
            jobExecution.Status = request.FinalStatus;
            jobExecution.EndedAt = DateTimeOffset.UtcNow;

            if (!string.IsNullOrEmpty(request.ErrorMessage))
                jobExecution.ErrorMessage = request.ErrorMessage;

            await writeRepository.UpdateJobExecutionAsync(jobExecution, ct);
        }
    }
}
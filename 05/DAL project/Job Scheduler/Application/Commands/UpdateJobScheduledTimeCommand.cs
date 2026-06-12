using Application.Extensions;
using Infrastructure.Data;
using MediatR;

namespace Application.Commands;

public record UpdateJobScheduledTimeCommand(Guid JobId) : IRequest<bool>;

public class UpdateJobScheduledTimeCommandHandler(SqlConnectionFactory connectionFactory)
    : IRequestHandler<UpdateJobScheduledTimeCommand, bool>
{
    public async Task<bool> Handle(UpdateJobScheduledTimeCommand request, CancellationToken ct)
    {
        var writeRepository = connectionFactory.GetJobStoreRepository(Domain.Enums.ConsistencyLevel.Strong);

        var job = await writeRepository.GetJobByIdAsync(request.JobId, ct);

        if (job == null)
            return false;

        var nextOCcurrence = job.CronExpression.GetNextExecution(afterTime: job.ScheduledTime);
        if (job.IsRecurrent
            && nextOCcurrence.HasValue)
        {
            job.ScheduledTime = nextOCcurrence.Value;

            await writeRepository.UpdateJobAsync(job, ct);
        }       

        return true;
    }
}
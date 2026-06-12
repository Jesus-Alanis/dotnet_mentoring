using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using MediatR;

namespace Application.Queries;

public record GetJobExecutionByIdQuery(Guid JobId, DateTimeOffset ScheduledTime) 
    : IRequest<JobExecution?>;

public class GetJobExecutionByIdQueryHandler(SqlConnectionFactory connectionFactory)
    : IRequestHandler<GetJobExecutionByIdQuery, JobExecution?>
{
    public async Task<JobExecution?> Handle(GetJobExecutionByIdQuery request, CancellationToken ct)
        => await connectionFactory.GetJobStoreRepository(ConsistencyLevel.Strong)
        .GetJobExecutionByIdAsync(request.JobId, request.ScheduledTime, ct);
}
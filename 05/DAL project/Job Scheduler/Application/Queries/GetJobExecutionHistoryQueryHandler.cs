using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using MediatR;

namespace Application.Queries;

public record GetJobExecutionHistoryQuery(Guid JobId, DateTimeOffset StartDate, DateTimeOffset EndDate, JobExecutionStatus? JobExecutionStatus = null)
    : IRequest<List<JobExecution>>;

public class GetJobExecutionHistoryQueryHandler(SqlConnectionFactory connectionFactory)
    : IRequestHandler<GetJobExecutionHistoryQuery, List<JobExecution>>
{
    public async Task<List<JobExecution>> Handle(GetJobExecutionHistoryQuery request, CancellationToken ct)
        => await connectionFactory.GetJobStoreRepository(ConsistencyLevel.Eventual)
        .GetJobExecutionHistoryAsync(request.JobId, request.StartDate, request.EndDate, request.JobExecutionStatus, ct);
}
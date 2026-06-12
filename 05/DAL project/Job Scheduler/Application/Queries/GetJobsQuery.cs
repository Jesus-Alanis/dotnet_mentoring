using Domain.Entities;
using Infrastructure.Data;
using MediatR;

namespace Application.Queries;

public record GetJobsQuery(Guid OwnerId, DateTimeOffset StartDate, DateTimeOffset EndDate, JobStatus? JobStatus = null)
    : IRequest<List<JobRecord>>;

public class GetJobsQueryHandler(SqlConnectionFactory connectionFactory)
    : IRequestHandler<GetJobsQuery, List<JobRecord>>
{
    public async Task<List<JobRecord>> Handle(GetJobsQuery request, CancellationToken ct)
    {
        return await connectionFactory.GetJobStoreRepository(Domain.Enums.ConsistencyLevel.ReadAfterWrite, request.OwnerId)
            .GetJobsByOwnerId(request.OwnerId, request.StartDate, request.EndDate, request.JobStatus, ct);
    }
}
using Domain.Entities;
using Infrastructure.Data;
using MediatR;

namespace Application.Queries;

public record GetJobByIdQuery(Guid OwnerId, Guid JobId) : IRequest<JobRecord?>;

public class GetJobByIdQueryHandler(SqlConnectionFactory connectionFactory)
    : IRequestHandler<GetJobByIdQuery, JobRecord?>
{
    public async Task<JobRecord?> Handle(GetJobByIdQuery request, CancellationToken ct)
    {
        return await connectionFactory.GetJobStoreRepository(Domain.Enums.ConsistencyLevel.ReadAfterWrite, request.OwnerId)
            .GetJobByIdAsync(request.JobId, ct);
    }
}
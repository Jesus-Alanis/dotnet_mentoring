using Domain.Enums;
using Infrastructure.Data;
using MediatR;

namespace Application.Queries;

public record GetJobScheduledTimeQuery(Guid JobId) : IRequest<DateTimeOffset>;

public class GetJobScheduledTimeQueryHandler(SqlConnectionFactory connectionFactory)
    : IRequestHandler<GetJobScheduledTimeQuery, DateTimeOffset>
{
    public async Task<DateTimeOffset> Handle(GetJobScheduledTimeQuery request, CancellationToken ct)
        => await connectionFactory.GetJobStoreRepository(ConsistencyLevel.Strong).GetJobScheduledTimeAsync(request.JobId, ct);
}
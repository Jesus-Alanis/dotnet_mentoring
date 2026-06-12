using Infrastructure.Data.Repositories;
using MediatR;

namespace Application.Commands;

public record ReleaseJobLockCommand(Guid JobId) : IRequest<bool>;

public class ReleaseJobLockCommandHandler(IJobLockRepository lockRepository)
    : IRequestHandler<ReleaseJobLockCommand, bool>
{
    public async Task<bool> Handle(ReleaseJobLockCommand request, CancellationToken ct)
        => await lockRepository.ReleaseJobLockAsync(request.JobId, Domain.Enums.ConsistencyLevel.Strong, ct);
}
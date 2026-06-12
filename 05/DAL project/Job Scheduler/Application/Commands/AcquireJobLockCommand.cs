using Domain.Entities;
using Infrastructure.Data.Repositories;
using MediatR;

namespace Application.Commands;

public record AcquireJobLockCommand(Guid JobId, string WorkerId) : IRequest<bool>;

public class AcquireJobLockCommandHandler(IJobLockRepository lockRepository) 
    : IRequestHandler<AcquireJobLockCommand, bool>
{
    public async Task<bool> Handle(AcquireJobLockCommand request, CancellationToken ct)
    {
        var jobLock = new JobLock
        {
            Id = JobLock.IdFormat(request.JobId),
            JobId = request.JobId,
            LockedByWorkerId = request.WorkerId,
            LockedAt = DateTimeOffset.UtcNow
        };

        return await lockRepository.TryAcquireJobLockAsync(jobLock, ct);
    }
}
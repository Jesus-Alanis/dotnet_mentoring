namespace Domain.Entities;

public class JobLock
{
    public required Guid Id { get; set; }
    public required string Region { get; set; } // Partition Key
    public bool IsLocked { get; set; }
    public required string LockedByWorkerId { get; set; }
    public DateTimeOffset? LockedAt { get; set; }

    // Concurrency Token for Quorum OCC
    public string? _etag { get; set; }
}

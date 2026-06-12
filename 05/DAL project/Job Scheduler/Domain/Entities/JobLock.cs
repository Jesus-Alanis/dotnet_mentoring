namespace Domain.Entities;

public class JobLock
{
    public static string IdFormat(Guid id) => string.Format("Lock_{0}", id);

    public required string Id { get; set; }
    public required Guid JobId { get; set; } // Partition Key
    public required string LockedByWorkerId { get; set; }
    public DateTimeOffset? LockedAt { get; set; }
    public int TimeToLive { get; set; } = 86400; // 24 hours in seconds    
}

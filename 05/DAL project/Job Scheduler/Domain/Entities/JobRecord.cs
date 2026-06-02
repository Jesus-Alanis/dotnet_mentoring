namespace Domain.Entities;

public class JobRecord
{
    public Guid Id { get; set; }
    public required Guid OwnerId { get; set; }
    public required string Name { get; set; }
    public required string CronExpression { get; set; }
    public string? Payload { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Active;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<JobExecution> Executions { get; set; } = [];
}

public enum JobStatus : byte
{
    Active,
    Inactive
}
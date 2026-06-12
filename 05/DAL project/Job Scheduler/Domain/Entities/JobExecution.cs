namespace Domain.Entities;

public class JobExecution
{
    public required Guid JobId { get; set; }
    public required DateTimeOffset ScheduledTime { get; set; }
    public required string WorkerNodeId { get; set; }
    public JobExecutionStatus Status { get; set; } = JobExecutionStatus.Running;
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EndedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public JobRecord? Job { get; set; }
}

public enum JobExecutionStatus: byte
{
    Running,
    Completed,
    Failed
}

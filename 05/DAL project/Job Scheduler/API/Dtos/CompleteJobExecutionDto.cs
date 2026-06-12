using Domain.Entities;

namespace API.Dtos;

public class CompleteJobExecutionDto
{
    public JobExecutionStatus FinalStatus { get; set; }
    public string? ErrorMessage { get; set; }
}

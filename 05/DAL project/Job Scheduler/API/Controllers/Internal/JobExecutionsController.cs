using API.Dtos;
using Application.Commands;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Internal;

[ApiController]
[Route("api/internal/[controller]")]
public class JobExecutionsController(IMediator mediator, ILogger<JobExecutionsController> logger) : Controller
{
    [HttpGet("{jobId}/execution/{scheduledTime}")]
    public async Task<IActionResult> GetExecution(Guid jobId, DateTimeOffset scheduledTime)
    {
        logger.LogInformation("GET /api/jobexecutions/{jobId}/execution/{scheduledTime} received", jobId, scheduledTime);

        var jobExecution = await mediator.Send(new GetJobExecutionByIdQuery(jobId, scheduledTime));

        return jobExecution is null
            ? NotFound() 
            : Ok(jobExecution);
    }

    [HttpPost("{jobId}/execute")]
    public async Task<IActionResult> ExecuteJob([FromHeader(Name = "X-Worker-Id")] string workerId, Guid jobId)
    {
        logger.LogInformation("POST /api/jobexecutions/{jobId}/execute received", jobId);

        var hasLock = await mediator.Send(new AcquireJobLockCommand(jobId, workerId));

        if (!hasLock)
        {
            // Return 409 Conflict if another worker is already processing it
            return Conflict(new { Message = "Job is already locked or executed by another worker." });
        }

        var scheduledTime = await mediator.Send(new GetJobScheduledTimeQuery(jobId));
        await mediator.Send(new StartJobExecutionCommand(jobId, scheduledTime, workerId));

        return CreatedAtAction(nameof(GetExecution), new { jobId, scheduledTime }, value: null);
    }

    [HttpPost("{jobId}/execution/{scheduledTime}/release")]
    public async Task<IActionResult> ReleaseJobLock(Guid jobId, DateTimeOffset scheduledTime, [FromBody] CompleteJobExecutionDto dto)
    {
        logger.LogInformation("POST /api/jobexecutions/{jobId}/execution/{scheduledTime}/release received", jobId, scheduledTime);

        await mediator.Send(new CompleteJobExecutionCommand(jobId, scheduledTime, dto.FinalStatus, dto.ErrorMessage));
        await mediator.Send(new UpdateJobScheduledTimeCommand(jobId));
        await mediator.Send(new ReleaseJobLockCommand(jobId));

        return Ok(new { Message = "Job lock released successfully." });
    }
}

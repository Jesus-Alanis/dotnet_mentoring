using Application.Commands;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController(IMediator mediator, ILogger<JobsController> logger) : Controller
{

    [HttpGet("{jobId}/user/{ownerId}")]
    public async Task<IActionResult> GetJob(Guid jobId, Guid ownerId)
    {
        logger.LogInformation("GET /api/jobs/{jobId}/user/{ownerId} received", jobId, ownerId);

        var job = await mediator.Send(new GetJobByIdQuery(ownerId, jobId));

        return job is null 
            ? NotFound() : Ok(job);
    }

    [HttpPost]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobCommand request)
    {
        logger.LogInformation("POST /api/jobs received");

        var jobId = await mediator.Send(request);

        return CreatedAtAction(nameof(GetJob), new { jobId, ownerId = request.OwnerId }, request);
    }
}

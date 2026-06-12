using Application.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IMediator mediator, ILogger<UsersController> logger) : Controller
{
    [HttpGet("{ownerId}/jobs")]
    public async Task<IActionResult> GetMyJobs(Guid ownerId, [FromQuery] DateTimeOffset startDate, [FromQuery] DateTimeOffset endDate, [FromQuery] JobStatus? jobStatus = null)
    {
        logger.LogInformation("GET /api/users/{ownerId}/jobs received", ownerId);

        var jobs = await mediator.Send(new GetJobsQuery(ownerId, startDate, endDate, jobStatus));
        return Ok(jobs);
    }
}
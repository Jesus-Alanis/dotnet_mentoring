using Application.Commands;
using Application.Queries;
using Demo.Workers;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();

// SQL Write Replica (Primary Leader)
builder.Services.AddDbContext<JobSqlDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("JobStore")));

// SQL Read Replica (Asynchronous Follower)
// Notice the "ApplicationIntent=ReadOnly" requirement in the connection string
builder.Services.AddDbContext<JobReadDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("JobStore")
        + ";ApplicationIntent=ReadOnly;"),
    contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Transient); // just for simplicity sake to allow concurrent dbcontext execution by creating different instances each time

// 3. Cosmos DB Lock Store (Quorum, Single Region)
builder.Services.AddDbContext<JobLockDbContext>(options =>
    options.UseCosmos(
        accountEndpoint: builder.Configuration["CosmosDb:Endpoint"]!,
        accountKey: builder.Configuration["CosmosDb:Key"]!,
        databaseName: "JobScheduler"), 
        contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Transient); // just for simplicity sake to allow concurrent dbcontext execution by creating different instances each time

// Register CQRS Handlers
builder.Services.AddScoped<CreateJobCommandHandler>();
builder.Services.AddScoped<GetActiveJobsQueryHandler>();
builder.Services.AddTransient<GetJobScheduledTimeQueryHandler>();
builder.Services.AddTransient<AcquireJobLockCommandHandler>();
builder.Services.AddTransient<ReleaseJobLockCommandHandler>();
builder.Services.AddTransient<UpdateJobScheduledTimeCommandHandler>();
builder.Services.AddTransient<JobExecutionCommandHandler>();
builder.Services.AddTransient<JobExecutionOrchestrator>();

using var host = builder.Build();

using var scope = host.Services.CreateScope();

var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
var userId = Guid.NewGuid();
var ct = new CancellationToken();

var jobId = await CreateJobAsync($"Job 100", isRecurrent: true);
await RunJobAsync(jobId);

await host.RunAsync();




async Task<Guid> CreateJobAsync(string jobName, bool isRecurrent)
{
    var createJobHandler = scope.ServiceProvider.GetRequiredService<CreateJobCommandHandler>();

    var jobId = await createJobHandler.HandleAsync(new CreateJobCommand(userId, jobName, "0 16 */3 8 *", isRecurrent, "{\"field_1\":\"field_value\"}"), ct);

    var getActiveJobsHandler = scope.ServiceProvider.GetRequiredService<GetActiveJobsQueryHandler>();

    var jobs = await getActiveJobsHandler.HandleAsync(new GetActiveJobsQuery(userId), ct);
    foreach (var job in jobs)
    {
        logger.LogInformation("Job Info - Id: {jobId}, Job Name: {jobName}, Job Status: {jobStatus}", job.Id, job.Name, job.Status);
    }

    return jobId;
}

async Task RunJobAsync(Guid jobId)
{
    // In theory, there should be a background worker scanning the Jobs table by looking up jobs scheduled time and invoking the job orchestrator.
    var getJobScheduledTimeHandler = scope.ServiceProvider.GetRequiredService<GetJobScheduledTimeQueryHandler>();

    var jobScheduledTime = await getJobScheduledTimeHandler.HandleAsync(new GetJobScheduledTimeQuery(jobId), ct);

    var jobExecutionOrchestrator1 = scope.ServiceProvider.GetRequiredService<JobExecutionOrchestrator>();
    var jobExecutionOrchestrator2 = scope.ServiceProvider.GetRequiredService<JobExecutionOrchestrator>();   

    await Task.WhenAll(jobExecutionOrchestrator1.ProcessJobAsync(jobId, jobScheduledTime, ct), jobExecutionOrchestrator2.ProcessJobAsync(jobId, jobScheduledTime, ct));
}
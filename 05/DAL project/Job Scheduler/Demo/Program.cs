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
builder.Services.AddTransient<AcquireJobLockCommandHandler>();
builder.Services.AddTransient<ReleaseJobLockCommandHandler>();
builder.Services.AddTransient<JobExecutionCommandHandler>();
builder.Services.AddTransient<JobExecutionOrchestrator>();

using var host = builder.Build();

using var scope = host.Services.CreateScope();

var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
var userId = Guid.NewGuid();
var ct = new CancellationToken();

var job1 = await CreateJobAsync("Important Job!");

await RunJobAsync(job1);

await host.RunAsync();




async Task<Guid> CreateJobAsync(string jobName)
{
    var createJobHandler = scope.ServiceProvider.GetRequiredService<CreateJobCommandHandler>();

    var jobId = await createJobHandler.HandleAsync(new CreateJobCommand(userId, jobName, "*/5 * * * *", "{\"field_1\":\"field_value\"}"), ct);

    var getActiveJobsHandler = scope.ServiceProvider.GetRequiredService<GetActiveJobsQueryHandler>();

    var jobs = await getActiveJobsHandler.HandleAsync(new GetActiveJobsQuery(userId), ct);
    foreach (var job in jobs)
    {
        logger.LogInformation("Job Info - Id: {jobId}, User Id: {userId}, Job Name: {jobName}, Job Status: {jobStatus}", job.Id, job.OwnerId, job.Name, job.Status);
    }

    return jobId;
}

async Task RunJobAsync(Guid jobId)
{
    var jobExecutionOrchestrator1 = scope.ServiceProvider.GetRequiredService<JobExecutionOrchestrator>();
    var jobExecutionOrchestrator2 = scope.ServiceProvider.GetRequiredService<JobExecutionOrchestrator>();

    await Task.WhenAll(jobExecutionOrchestrator1.ProcessJobAsync(jobId, "worker ID 1", "US", ct), jobExecutionOrchestrator2.ProcessJobAsync(jobId, "worker ID 2", "US", ct));
}
using Application.Commands;
using Application.Extensions;
using Application.Queries;
using Demo.Workers;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddDbContext<JobStoreDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("JobStore")));

builder.Services.AddDbContext<JobLockDbContext>(options =>
    options.UseCosmos(
        accountEndpoint: builder.Configuration["CosmosDb:Endpoint"]!,
        accountKey: builder.Configuration["CosmosDb:Key"]!,
        databaseName: "JobScheduler"), 
        contextLifetime: ServiceLifetime.Transient); // just for simplicity sake to allow concurrent dbcontext execution by creating different instances each time

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IJobLockRepository, JobLockRepository>();
builder.Services.AddScoped<IConsistencyManager, ConsistencyManager>();
builder.Services.AddScoped<SqlConnectionFactory>();

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(StringExtensions).Assembly);
});

using var host = builder.Build();

await host.RunAsync();
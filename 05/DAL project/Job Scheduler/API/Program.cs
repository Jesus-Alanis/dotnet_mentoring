using Application.Extensions;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddDbContext<JobLockDbContext>(options =>
    options.UseCosmos(
        accountEndpoint: builder.Configuration["CosmosDb:Endpoint"]!,
        accountKey: builder.Configuration["CosmosDb:Key"]!,
        databaseName: "JobScheduler"));

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IJobLockRepository, JobLockRepository>();
builder.Services.AddScoped<IConsistencyManager, ConsistencyManager>();
builder.Services.AddScoped<SqlConnectionFactory>();

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(StringExtensions).Assembly);
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

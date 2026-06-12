using Domain.Enums;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public class SqlConnectionFactory(IConfiguration config, IConsistencyManager consistencyManager, ILogger<SqlConnectionFactory> logger)
{
    public IJobStoreRepository GetJobStoreRepository(ConsistencyLevel consistencyLevel, Guid? userId = null)
    {
        logger.LogInformation("Resolving SQL connection for UserId: '{UserId}' with Strategy: {ConsistencyLevel}",
            userId?.ToString() ?? "SYSTEM", consistencyLevel);

        string nodeType;
        string selectedConnectionString;

        switch (consistencyLevel)
        {
            case ConsistencyLevel.Strong:
                nodeType = "PRIMARY_NODE";
                selectedConnectionString = config.GetConnectionString("JobStore")!;
                break;

            case ConsistencyLevel.Eventual:
                nodeType = "REPLICA_NODE";
                selectedConnectionString = config.GetConnectionString("JobStore") + ";ApplicationIntent=ReadOnly;";
                break;

            case ConsistencyLevel.ReadAfterWrite:
                if (ShouldRouteToPrimary(userId))
                {
                    nodeType = "PRIMARY_NODE (Read-After-Write Cooldown Active)";
                    selectedConnectionString = config.GetConnectionString("JobStore")!;
                }
                else
                {
                    nodeType = "REPLICA_NODE (Read-After-Write Cooldown Expired)";
                    selectedConnectionString = config.GetConnectionString("JobStore") + ";ApplicationIntent=ReadOnly;";
                }
                break;

            default:
                nodeType = "PRIMARY_NODE (Fallback)";
                selectedConnectionString = config.GetConnectionString("JobStore")!;
                break;
        }

        logger.LogInformation(">>> DATABASE ROUTING DECISION: Traffic routed to {NodeType} <<<", nodeType);

        var optionsBuilder = new DbContextOptionsBuilder<JobStoreDbContext>();
        optionsBuilder.UseSqlServer(selectedConnectionString);

        return new JobStoreRepository(new JobStoreDbContext(optionsBuilder.Options));
    }

    private bool ShouldRouteToPrimary(Guid? userId)
    {
        if (!userId.HasValue) return true; // Fail safe to strong if no context
        return consistencyManager.IsReadAfterWriteApplicable(userId.Value);
    }
}

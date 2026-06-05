Unlike Cosmos DB where partitioning is handled at the resource deployment level, SQL partitioning is configured at the schema level using T-SQL.

### Step 1: Create a Partition Function

The partition function defines the boundary values (e.g., daily or monthly boundaries). We shoud use RANGE RIGHT to indicate that the boundary value belongs to the right side of the interval.


```
-- Creates monthly partitions for the year 2026. 

CREATE PARTITION FUNCTION pf_JobScheduledTime (DATETIME2)
AS RANGE RIGHT FOR VALUES (
    '2026-06-01T00:00:00', 
    '2026-07-01T00:00:00', 
    '2026-08-01T00:00:00'
    -- Add future boundaries as needed
);
```

We could automate the creation of future boundaries using a SQL Agent Job or Azure Automation.

### Step 2: Create a Partition Scheme

The partition scheme maps the partitions created by the function to filegroups. In Azure SQL Database, we typically map everything to the [PRIMARY] filegroup since Azure manages the underlying storage automatically.

```
CREATE PARTITION SCHEME ps_JobScheduledTime
AS PARTITION pf_JobScheduledTime ALL TO ([PRIMARY]);
```

### Step 3: Create the Partitioned Table

When creating the table, you must map it to the partition scheme and specify the ScheduledTime as the partition key.

```
CREATE TABLE dbo.JobExecution (
    job_id UNIQUEIDENTIFIER NOT NULL,
    status VARCHAR(20) NOT NULL,
    scheduled_time DATETIME2 NOT NULL,
    started_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    -- In partitioned tables, the clustered index
    -- MUST include the partition key column.
    CONSTRAINT PK_JobExecution PRIMARY KEY CLUSTERED (job_id, scheduled_time)
) ON ps_JobScheduledTime(scheduled_time);
```

To keep performance high and storage costs down, we could periodically use the `ALTER TABLE ... SWITCH PARTITION` command to instantly move old, completed jobs from the active table to a cheap, cold-storage archive table.





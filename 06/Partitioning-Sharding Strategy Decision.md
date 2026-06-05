# UC 1.1: Create a New Job

## 1) Analyze data growth and access patterns:

* Expected data growth rate (records per day/month)

I assume an enterprise system should expect roughly 500,000 to 1,000,000 new jobs created per day (~15 to 30 million records per month).

At ~2 KB per job record (including payload and metadata), this equates to 1 to 2 GB per day (30 - 60 GB per month).

* Query patterns (time-based queries, user-based queries, etc.)

Time-based Range Queries (Heavy Reads): The Job Orchestrator constantly scans for jobs where scheduled_time <= NOW() and Status == 'Active'.

User-based Queries: Users querying the dashboard to see their job history (SELECT * FROM Jobs WHERE user_id = 'X' ORDER BY CreatedAt DESC).

## 2) Decide on partitioning vs sharding approach and justify:

* Should you use partitioning (single database instance) or sharding (multiple database instances)?

Partitioning (Single database instance using Table Partitioning)

Azure SQL Hyperscale can easily handle up to 100 TB of data in a single instance, avoiding massive operational complexity, distributed transactions, and cross-shard querying issues introduced by sharding.

* For sharding: what number of shards is required?

No sharding is required.

* What partitioning/sharding strategy will you choose for your database? (range-based, hash-based, list-based, etc.)

Range-based Partitioning, expecting heavy reads by the job orchestrator

* What partition/shard key will you choose?

scheduled_time

By partitioning the table by Scheduled Time (e.g., creating daily or weekly partitions), the SQL Server optimizer uses Partition Elimination. Instead of scanning millions of rows, it only scans today's specific partition.

# UC 2.1: Execute a Job At a Scheduled Time

## 1) Analyze data growth and access patterns:

* Expected data growth rate (records per day/month)

A few GBs, the snapshots store only holds active lock/snapshots, deleting records after the job execution completes.

* Query patterns (time-based queries, user-based queries, etc.)

Atomic Writes: the job orchestrator attempts to INSERT a lock document. This is a concurrent, point-write pattern expecting HTTP 201 (Success) or HTTP 409 (Conflict).

No range queries; exclusively key-value access.

## 2) Decide on partitioning vs sharding approach and justify:

* Should you use partitioning (single database instance) or sharding (multiple database instances)?

Sharding (Multiple physical database nodes, abstracted by Cosmos DB).

Executing jobs requires handling sudden bursts of concurrent workers fighting for locks at the exact same millisecond. A single database instance would become an IOPS bottleneck. 

Cosmos DB automatically shards data across multiple physical servers to guarantee single-digit millisecond latency for reads and writes.

* For sharding: what number of shards is required?

Cosmos DB handles this automatically, but physically, we could configure the container to start with enough Request Units (RUs) to provision 3 to 5 physical shards to handle high-concurrency write spikes at the top of every minute.

* What partitioning/sharding strategy will you choose for your database? (range-based, hash-based, list-based, etc.)

Hash-based Sharding.

* What partition/shard key will you choose?

job_id

By routing the lock creation to a specific Job Id shard, the database engine can strictly enforce the Unique ID constraint, allowing the first worker to acquire the lock (HTTP 201) and rejecting the remaining workers (HTTP 409 Conflict), completely preventing the "Double Execution" problem without distributed locking deadlocks.

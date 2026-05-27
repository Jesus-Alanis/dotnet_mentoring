# UC 1.1
Recommended Level: READ COMMITTED (Default in Azure SQL)

Creating a job is primarily an INSERT operation into the JOB table, validating against the USER table. READ COMMITTED prevents dirty reads, higher isolation levels (like Serializable) would just cause unnecessary locking overhead and degrade API write performance.

# UC 2.1
this use case spans a NoSQL database and a relational database

1. Acquiring the Lock in Cosmos DB (Snapshot Store)

Recommended Approach: Optimistic Concurrency Control using ETags.

When the Orchestrator tries to lock a JOB_SNAPSHOT, it reads the document's _etag. When it writes the update (is_locked = true), Cosmos DB checks if the _etag has changed. If another orchestrator node grabbed it first, the update fails, preventing race conditions.

2. Writing the Execution Record in Azure SQL (Job Store)

Recommended Level: READ COMMITTED

When the Runner finishes and writes the final JOB_EXECUTION record, it is an INSERT or an UPDATE to a specific row owned by that runner. There is no risk of phantom reads or lost updates because only one Runner (guaranteed by the Cosmos DB lock) is modifying that specific execution record.
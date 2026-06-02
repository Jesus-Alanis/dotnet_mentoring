# UC 1.1: Create a New Job

## Formulate system component requirements:

* Expected data volume (number of records, data size in GB)

I assumed a moderately large enterprise platform with 10 million active scheduled jobs. A typical job record (ID, owner, cron expression, payload, timestamps) takes up roughly 2 Kilobytes (KB) of space., the total active data volume will be around 20GB.

Calculation: 10,000,000 jobs × 2 KB = 20,000,000 KB ≈ 20 GB.

* Expected load (read/write requests per second)

Job creation is a human or CI/CD driven process. Even in a massive system, 100 to 500 Writes for new jobs/updates per second and Reads should be higher (~1,000/sec) because dashboards continuously poll to see job lists.

* Consistency requirements (strong or eventual consistency)

Strong consistency. If an user creates a job and the UI immediately refreshes to a read-replica that hasn't synced yet (eventual consistency), the job will appear to be missing, causing the admin to panic or create duplicates.

* Availability requirements (uptime expectations, acceptable downtime)

High (99.99% uptime). Job creation is a critical path for user experience, translating to a maximum acceptable downtime of ~4.38 minutes per month.

* Geographic distribution (single region or multi-region deployment)

Multi-region deployment. A primary region handles writes, while secondary regions handle local reads and act as a disaster recovery fallback.

## Select the most suitable database and justify your choice:

* Which database type is most appropriate: SQL or NoSQL?

SQL (Azure SQL).

* What are the advantages and disadvantages of the selected database given these requirements?

Advantages: Provides ACID guarantees and strong schema enforcement. This is critical for job data, ensuring that cron expressions, owner IDs, and status fields remain structured and valid. SQL easily supports secondary indexes, making it simple to query jobs by user, status, or schedule.

Disadvantages: Vertical and horizontal write scaling can be more challenging than NoSQL.

* Deployment approach: self-hosted or cloud service?

Cloud service. This removes the operational burden of patching, backups, and manually managing replication clusters.

## Design the replication strategy for the selected database:

* Justify why replication is required for this system component

Replication is necessary to ensure fault tolerance (preventing data loss if a node fails) and to offload read traffic (like admin dashboards) from the primary node.

* Describe the replication strategy:

    * For cloud service databases: Identify the default replication strategy and how you will configure it
    * For self-hosted databases: Specify which replication strategy you will implement (leader-follower, multi-leader, or leaderless)

        Regardless of cloud service or self-hosted DBs, the replication strategy would be the same:
        
        Leader-Follower (Primary-Replica). All new job creations (writes) will be routed to the Leader.


* Define the replication configuration parameters (number of replicas, synchronous/asynchronous mode, quorum settings if applicable)

**Number of replicas**: 1 local secondary replica (different Availability Zone) + 1 remote read replica (different Region).

**Mode**: Synchronous for the local AZ replica to ensure zero data loss during local node failure. Asynchronous for the cross-region read replica to avoid punishing the primary write latency over long geographical distances.



# UC 2.1: Execute a Job At a Scheduled Time

## Formulate system component requirements:

* Expected data volume (number of records, data size in GB)

Low size, highly transient. The lock store only holds active lock/snapshots (a few GBs)

* Expected load (read/write requests per second)

High. If just 5% of 10 million jobs are scheduled for 12:00 PM, that's 500,000 jobs. If the job orchestrator workers try to acquire locks for all 500,000 jobs within the first 60 seconds of that minute, that requires 500,000 / 60 = ~8,333 write requests per second.

* Consistency requirements (strong or eventual consistency)

Strong consistency for lock acquisition. Strict serialization is required to prevent the "Double Execution" problem (two nodes picking up the same job).

* Availability requirements (uptime expectations, acceptable downtime)

Extremely High (99.999% uptime). If the lock store fails, job execution halts entirely.

* Geographic distribution (single region or multi-region deployment)

Confined to a Single Region (or localized cluster) for execution. Acquiring a lock across regions would introduce unacceptable latency

## Select the most suitable database and justify your choice:

* Which database type is most appropriate: SQL or NoSQL?

NoSQL (Azure Cosmos DB).

* What are the advantages and disadvantages of the selected database given these requirements?

Advantages: NoSQL systems are highly scalable for immense read/write loads. Cosmos DB provide features like Optimistic Concurrency Control using ETags, allowing atomic lock acquisition without heavy table-level locking.

Disadvantages: Lacks relational joins, which are unnecessary for simple Key-Value lock acquisitions or execution log ingestion.

* Deployment approach: self-hosted or cloud service?

Cloud service. Managing a highly available distributed consensus cluster (like ZooKeeper, etcd, or a NoSQL ring) manually is notoriously complex

## Design the replication strategy for the selected database:

* Justify why replication is required for this system component

Job snapshots requires consensus to survive node failures without locking up the entire system. Replication guarantees that if a leader or storage partition goes down while holding locks, another node can securely resume the state.

* Describe the replication strategy:

    * For cloud service databases: Identify the default replication strategy and how you will configure it
    * For self-hosted databases: Specify which replication strategy you will implement (leader-follower, multi-leader, or leaderless)

        Regardless of cloud service or self-hosted DBs, the replication strategy would be the same:

        Leaderless (Quorum). Since we are using a cloud-native NoSQL database (Cosmos DB), it abstracts the replication into a replica set where operations must be acknowledged by a majority to guarantee consistency.


* Define the replication configuration parameters (number of replicas, synchronous/asynchronous mode, quorum settings if applicable)

**Number of replicas**: Minimum 3 to 5 nodes (managed under the hood by the provider) to establish a mathematical majority.

**Mode**: Synchronous via Quorum. To achieve the strong consistency required for locks, writes cannot be fully asynchronous.

**Quorum settings**: Write Quorum = Majority (e.g., 2/3 nodes) and Read Quorum = Majority. Using a strict quorum ($W + R > N$) guarantees that reading the lock document's _etag will always reflect the most recent update. If another runner attempts to update is_locked = true simultaneously, the ETag check will fail and safely prevent a race condition.
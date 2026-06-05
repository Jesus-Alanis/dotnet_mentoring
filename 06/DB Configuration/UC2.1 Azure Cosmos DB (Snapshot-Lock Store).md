For Cosmos DB, I chose Hash-based Sharding on the job_id key and we could also rely on the database's Time-To-Live (TTL) feature to purge stale locks by the end of the day.

This is configured when provisioning the container.

### Step 1: Provisioning a Cosmos DB account

* Navigate to your Cosmos DB Account in the Azure Portal.

* Click Data Explorer -> New Container.

* Fill in the configuration:

    * Database id: JobScheduler

    * Container id: JobSnapshots

    * Partition key: /job_id (This is case-sensitive and critical for the atomic lock guarantee).

* Expand the Advanced section (or go to Settings after creation).

* Find Time to Live (TTL) and set it to On.

* Enter 86400 seconds (24 hours).


### Step 2: Application-Side Validation

When worker nodes attempt to execute a job, they will leverage this sharding structure to atomically acquire the lock
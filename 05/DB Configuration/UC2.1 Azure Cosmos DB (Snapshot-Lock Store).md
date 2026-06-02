Replication Strategy: Leaderless/Quorum (Single Region, Strong Consistency)

To prevent the "Double Execution" problem, the lock store must strictly serialize operations. 
This requires guaranteed quorum reads and writes.


Step 1: Provision a Single-Region Account with Availability Zones

Strong consistency in Cosmos DB is explicitly blocked if you enable multi-region writes, as cross-region strong consistency would cause massive performance degradation.  

* Sign in to the Azure Portal and navigate to Azure Cosmos DB.  
* Click + Create and select Azure Cosmos DB for NoSQL.
* Under the Basics tab, choose your primary Location (e.g., East US).
* Under Global Distribution, ensure that Multi-region Writes is set to Disable.
* Ensure that Availability Zones is set to Enable.

    * This provisions a 4-node replica set spread across physical zones in your single region. It creates the physical infrastructure necessary for the quorum to survive a node/zone failure while keeping latency low.


Step 2: Enforce Strong Consistency

By default, Cosmos DB provisions accounts with Session consistency. You must elevate this to Strong.  

* Once the Cosmos DB account is deployed, navigate to it in the portal.  
* In the left-hand menu under Settings, select Default consistency.
* You will see a visualization with music notes showing how data propagates. 
Select the Strong option on the far left.
* Click Save.

    * This changes the underlying database behavior. Now, any read or write to the lock document will be blocked until a strict majority of the replicas acknowledge the commit.


Step 3: Implement Optimistic Concurrency Control (Code Level)

While the database is now configured for strong quorum consensus, the actual "locking" mechanism must use Cosmos DB's ETags at the application SDK level.
Replication Strategy: Leader-Follower (Synchronous Local + Asynchronous Cross-Region)

To achieve zero data loss locally and asynchronous disaster recovery across regions, a combination of Zone Redundancy and Active Geo-Replication is required.


Step 1: Enable Zone Redundancy (Local Synchronous Replica)

Zone redundancy automatically provisions secondary replicas across different physical data centers (Availability Zones) within the same region, utilizing synchronous replication.

* Sign in to the Azure Portal and navigate to SQL databases.  
* Click + Create to provision a new database. 
* Fill out the basic server details, then navigate to the Compute + Storage tab.
* Select Configure database.
* Locate the setting: "Would you like to make this database zone redundant?"
* Select Yes and click Apply / Review + Create.

    * If the primary rack/zone fails, Azure automatically promotes a local secondary replica with an RPO (Recovery Point Objective) of zero.


Step 2: Configure Active Geo-Replication (Cross-Region Asynchronous Replica)

This creates the read-only replica in a different geographic region to serve read traffic (like dashboards) and act as a disaster recovery fallback.

* Navigate to the newly created Azure SQL Database in the portal.  
* In the left-hand menu under Data management, select Replicas.  
* Click + Create Replica.
* In the configuration blade, select your Target Region
* Specify a Secondary Server (create a new one in the target region if you don't have one).  
* Click Review + Create.

    * Once created, Azure performs an initial seed. Following that, all committed transactions on the primary are asynchronously replicated. 
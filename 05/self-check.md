# Replication Lag

Q1.1: What is replication lag, and why does it occur?

Replication lag is the delay that occurs in an asynchronous replication system between the time a write is performed on the leader node and the time that change is successfully applied to a follower replica

Specific reasons for this delay include:

* **Asynchronous Propagation**: The leader sends changes to followers in the background, which inherently creates a window where the follower is not yet up-to-date
* **Network Failures or Latency**: Issues such as network partitions, flapping routes, or slow network links can delay the arrival of updates at replicas
* **Server Outages**: If a replica node goes down or is temporarily unreachable, it will fall behind and must catch up once it recovers
* **Resource Contention**: High traffic or heavy workloads on the leader or followers can slow down the replication process

Q1.2: What is one way to detect replication lag in a production environment?

One effective method to detect replication lag and inconsistencies between replicas is the use of Merkle trees (hash trees).
Each node maintains a Merkle tree for its key ranges, where the leaves are hashes of individual keys and parent nodes are hashes of their children
By comparing the root hashes of these trees, nodes can quickly determine if their data is in sync; if the root hashes differ, the nodes traverse the tree to identify specifically which keys are "out of sync" due to lag or failure

Q1.3: Name one strategy to mitigate the negative impact of replication lag.

A common strategy to mitigate the impact of lag—specifically to ensure a user sees their own updates—is "sticking to master"
This involves forcing the application to route all read queries to the leader (master) for a specific period (e.g., 10 seconds) after the user has performed a write
This ensures that the user's subsequent reads are not directed to a laggy replica that lacks the most recent data

Q1.4: What is a potential application-level consequence of replication lag?

A significant consequence is the loss of "read-your-writes" consistency, leading to a poor or confusing user experience
For example, a user might update a slide in a presentation or add an item to a shopping cart and receive a "success" confirmation; however, if they are immediately redirected to a view page and the read query is routed to a laggy replica, their changes will appear to be missing



# Leader-Follower (Master-Slave) Replication

Q2.1: In a leader-follower replication setup, why might an application choose asynchronous replication over synchronous replication?

Applications typically choose asynchronous replication to prioritize performance and system availability
In an asynchronous model, the leader confirms a write operation to the client immediately without waiting for acknowledgments from followers, which results in significantly lower latency
Furthermore, this approach increases fault tolerance; if a follower becomes slow or unreachable due to a network partition, the leader can continue processing writes uninterrupted, whereas a synchronous setup would block all new writes until the follower recovered

Q2.2: How is failover typically handled if the leader fails in a leader-follower system?

Failover is the process of designating a follower as the new leader when the original leader becomes unavailable.
In consensus-driven systems like Raft or Paxos, a leader election is triggered where candidates seek votes from a majority of servers; this majority requirement ensures that the new leader is aware of all previously committed operations and will not accidentally overwrite data

Q2.3: What is one major advantage of leader-follower replication?

A major advantage is the ability to scale the system's read capacity
Because read queries can be distributed across multiple replicas rather than hitting a single machine, the system can handle a much higher volume of concurrent read requests

Q2.4: Give one reason an organization might still prefer a single-leader approach despite scalability concerns.

An organization might prefer a single-leader approach because it is the most effective way to handle strong consistency requirements, such as uniqueness constraints
Since all writes pass through a single coordinator, it is straightforward to ensure that two users do not claim the same username or that a specific data constraint is never violated
In contrast, decentralized multi-leader or leaderless systems often require far more complex logic to detect and resolve conflicting updates to the same record

# Multi-Leader Replication

Q3.1: In multi-leader replication, why do conflicts occur more frequently than in leader-follower systems?

Conflicts occur more frequently in multi-leader systems because multiple leader nodes accept write operations independently and concurrently.
a multi-leader system allows different users to modify the same piece of data at different nodes simultaneously
Because these changes are asynchronously propagated to other leaders later, the system only detects the conflict after the data has already been saved to different local databases

Q3.2: How can applications handle conflicting writes in a multi-leader setup?

Applications use several strategies to manage or resolve these conflicts:

* **Conflict Avoidance**: The application ensures all writes for a specific record are routed to the same leader node (e.g., based on a user's geographic location), which prevents concurrent updates from happening in the first place
Convergent Resolution (Last Write Wins): Each write is tagged with a unique identifier or timestamp, and the system automatically accepts the newest version while discarding older ones
* **Application-Driven Logic**: The system delegates resolution to the application layer, where custom handlers merge or prioritize the conflicting data (for example, merging items in a shopping cart) before writing the final result back to the database
CRDTs (Conflict-Free Replicated Data Types): Use specialized data structures designed to automatically and safely merge concurrent updates without requiring manual intervention or complex locks
* **Vector Clocks**: These are used to capture the causal history of data versions, helping the system determine if one update is an ancestor of another or if they are truly independent conflicts requiring reconciliation

Q3.3: What is a typical use case for multi-leader replication?

Typical use cases include:

* **Multi-Datacenter Applications**: Distributing leaders across different geographical regions to handle local writes, which significantly reduces latency for global users
* **Offline-First Applications**: Mobile or desktop apps (like note-taking tools) where the device itself acts as a local leader, allowing the user to work without an internet connection and syncing changes once reconnected
* **Collaborative Editing**: Real-time tools like Google Docs where multiple users edit different sections of a document simultaneously, requiring asynchronous background syncs

Q3.4: What is the main reason some systems choose multi-leader replication despite the complexity of conflict resolution?

The main reason is to achieve an "always writeable" system that prioritizes high availability and low latency.
multi-leader replication allows the system to continue accepting updates even during outages or high-latency periods between datacenters
For businesses where rejecting a customer’s update (such as adding an item to a shopping cart) results in a poor experience or lost revenue, the performance and reliability gains outweigh the complexity of resolving eventual data inconsistencies



# Leaderless Replication

Q4.1: How does leaderless replication achieve consistency without a single leader node?

In a leaderless system, consistency is maintained through a quorum-based approach rather than a central coordinator
The system uses three configurable parameters: N (the total number of replicas), W (the minimum nodes required for a successful write), and R (the minimum nodes required for a successful read).
By setting these values such that R + W > N, the system ensures that the set of nodes participating in a read and the set of nodes participating in a write will overlap.
This overlap guarantees that at least one node in any read operation will possess the most recent version of the data, which the client can identify using versioning and vector clocks

Q4.2: What role does "hinted handoff" play in leaderless replication systems?

In a leaderless system, consistency is maintained through a quorum-based approach rather than a central coordinator
The system uses three configurable parameters: N (the total number of replicas), W (the minimum nodes required for a successful write), and R (the minimum nodes required for a successful read)
By setting these values such that R + W > N, the system ensures that the set of nodes participating in a read and the set of nodes participating in a write will overlap
This overlap guarantees that at least one node in any read operation will possess the most recent version of the data, which the client can identify using versioning and vector clocks

Q4.3: What is read repair, and why is it important in leaderless replication?

Read repair is an opportunistic synchronization process that occurs during a read operation
When a coordinator node receives multiple responses for a key, it may find that some replicas are stale compared to others
After returning the most recent version to the client, the coordinator pushes the latest data back to the lagging replicas to bring them up to date
This is vital because it ensures eventual consistency and significantly reduces the workload on background anti-entropy protocols (like Merkle tree synchronization), as many inconsistencies are fixed simply through the normal course of data access

Q4.4: How does setting W = N (where N is the total number of replicas) impact write availability in a leaderless system?

Setting W = N severely decreases write availability
While this configuration provides a very high level of durability and consistency—because a write is only confirmed once it is persisted on every single replica—it leaves the system vulnerable to failure
If even a single node in the replica set is offline or unreachable due to a network issue, the minimum requirement of W nodes cannot be met, and the system must reject the write request
Consequently, the system sacrifices its "always writeable" goal for the sake of strict data distribution across all nodes



# Practical Replication Considerations



Q5.1: Which replication strategy (leader-follower, multi-leader, or leaderless) typically prioritizes availability over strong consistency?

Both multi-leader and leaderless replication typically prioritize high availability and low latency over strong consistency.
**Leaderless replication**, in particular, uses a quorum-based approach that allows the system to continue processing writes even during server outages or network failures, provided a minimum number of nodes respond
Similarly, **multi-leader replication** is often selected for multi-datacenter or offline-first applications because it allows each leader to accept local writes independently, ensuring the system remains available even when datacenters are disconnected from one another


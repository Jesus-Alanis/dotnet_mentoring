# Consistency Models

Q1.1: What are the trade-offs between strong and weak consistency in distributed systems?

Distributed systems face fundamental trade-offs between performance (latency and throughput) and correctness (data accuracy)

* Strong Consistency: This model ensures that every read returns the most recent write, providing a single, up-to-date view of the data
    * Pros: It offers predictable behavior, prevents stale reads, and is easy to reason about for developers
    * Cons: It is expensive to implement because it requires synchronization and coordination between nodes.
    This increases latency, reduces throughput, and lowers availability during network partitions

* Weak Consistency: This is the least strict model, where replicas may diverge significantly without immediate convergence
    * Pros: It prioritizes high availability and low latency, supporting fast performance and simultaneous updates
    * Cons: It provides the least guarantees; it does not promise when or even if replicas will become consistent, which can lead to data discrepancies

Q1.2: What is causal consistency, and how does it differ from eventual consistency?

* Causal Consistency: This model preserves cause-and-effect relationships by ensuring that related (causally dependent) operations are seen in the same order across all nodes
If operation A happens before B, every user who observes A must also observe B
It is more scalable than strong consistency but ensures meaningful data relationships

* Eventual Consistency: This is the weakest form of consistency
It guarantees that if no new writes occur, all replicas will eventually converge to the same value
However, it offers no ordering guarantees on reads and writes; clients may temporarily see different or stale values until synchronization is complete


Q1.3: Which consistency model ensures that any read returns the result of the most recent write across the system (i.e., no stale data)?

**Strong Consistency** (also referred to as linearizability or strict consistency) is the model that ensures any read returns the result of the most recent committed write across the system
This guarantee means a client never sees uncommitted, partial, or stale data

Q1.4: Which consistency model provides only eventual convergence of replicas without guaranteeing immediacy of writes on reads?

**Eventual Consistency** provides only the eventual convergence of replicas without guaranteeing the immediacy of writes on reads
It relaxes strict consistency to improve system availability and performance, acknowledging that data might temporarily differ across replicas

Q1.5: What is the Read-Your-Writes (RYOW) session guarantee, and in what scenarios is it essential?

The Read-Your-Writes guarantee (a type of session consistency) ensures that once a client updates a piece of data, they will always see that latest change in any of their subsequent reads

This guarantee is essential in user-centric scenarios where immediate feedback on one's own actions is required to avoid confusion

Key examples include:

* Social Media: A user expects to see their own post or comment immediately after hitting "publish"
* Profile Updates: When a user changes their profile picture or information, they expect to see the updated version upon refreshing the page
* E-commerce: After adding an item to a shopping cart, the user needs to see that item reflected in their cart immediately
* Email: If a user deletes an email, RYOW ensures they do not see that same email reappear after refreshing their inbox


# Linearizability and Serializability

Q2.1: What is the difference between linearizability and serializability?

The key differences between the two are summarized below:

#### Scope of Guarantee
* Linearizability is a guarantee about single operations on a single object
It ensures that an operation on an object (like a counter or a data record) appears to occur instantaneously and exactly once between its start and end times
* Serializability is a high-level isolation guarantee for groups of operations, known as transactions, which often involve multiple objects
It ensures that the final state of the system is the same as if these transactions had been executed one after another in some sequential order

#### Real-Time (Ordering) Constraints
* Linearizability provides real-time (wall-clock) guarantees
If one operation completes before another starts, the system must reflect that order
It acts like a single, physical queue where everyone is served in the exact order they arrived
* Serializability does not provide real-time ordering
While it ensures transactions are isolated and results are consistent with some sequential execution, that sequence does not necessarily have to follow the actual chronological order in which the transactions were initiated or completed

#### Typical Scenarios and Usage
* Linearizability is essential for scenarios requiring strong consistency on individual actions, such as leader election (ensuring only one node succeeds) or distributed counters
* Serializability is the standard for maintaining integrity across complex workflows, such as banking transactions (e.g., transferring money between two accounts) or inventory management (checking stock, decrementing count, and confirming an order as a single unit)


# Consensus Algorithms

Q3.1: What is consensus in distributed systems, and why is it important for high-load applications?

Consensus is the process by which multiple computers in a distributed network agree on a single data value or decision.

For high-load applications, consensus is critical for several reasons:

* Data Consistency: It ensures all users and system nodes read the same data value after an update, preventing conflicting or mismatched data

* Fault Tolerance: It allows a system to continue operating correctly even if some nodes crash or network partitions occur

* Coordination and Synchronization: It is essential for coordinated actions, such as leader election or managing shared resources like locks

* Scalability: It helps manage increasing amounts of work by providing a structured way for a growing number of nodes to reach agreement

Q3.2: What are the main challenges in achieving consensus in a distributed system?

* Fault Tolerance: Systems must handle "Crash Faults" (node failures) and, in high-security settings, "Byzantine Faults" (malicious or arbitrary node behavior)

* Scalability and Message Overhead: Many algorithms require extensive communication between nodes; as node counts grow, message complexity can cause network congestion and high latency

* Synchronization Issues: Variations in network latency can lead to nodes having different views of the system, and asynchronous clocks can make it difficult to order events correctly

* Security Threats: Algorithms must be robust against Sybil attacks (fake identities), double-spending in financial systems, and Denial-of-Service (DoS) attacks

* Configuration Management: Handling "Dynamic Membership"—nodes joining or leaving the network—without disrupting the consensus process is difficult

Q3.3: What is a quorum in distributed consensus, and why is the R+W>N formula important?

A quorum is the minimum number of nodes (typically a majority) required to agree on a value or commit a transaction

The R + W > N formula is fundamental to quorum consensus, where:
N = The total number of replicas/nodes.
R = The number of nodes required for a read quorum.
W = The number of nodes required for a write quorum

This formula is important because it ensures that read and write quorums always intersect
Because of this overlap, any read quorum is guaranteed to include at least one node that was part of the most recent write quorum, ensuring the client receives the most recent data and avoids stale reads

Q3.4: What is the Raft consensus algorithm, and why was it developed as an alternative to Paxos?

Raft is a leader-based consensus algorithm that manages log replication across nodes
It was developed in 2013 as a direct alternative to the older Paxos algorithm

Raft was created primarily because Paxos was widely considered too complex to understand and implement correctly
Raft improves on Paxos by:

* Simplicity: It uses a more intuitive leader-based approach where a dedicated leader manages the log and client requests

* Decomposition: It breaks the consensus problem into three manageable sub-problems: leader election, log replication, and safety

* Clear Guidelines: It provides explicit log management and clearer implementation rules, leading to its widespread adoption in modern systems like etcd, Consul, and CockroachDB


# Distributed Transactions

Q4.1: What is a distributed transaction, and why do such transactions require special protocols?

A distributed transaction is a set of data operations performed across two or more data repositories, such as separate databases
These transactions aim to maintain ACID (atomicity, consistency, isolation, durability) principles across multiple nodes, ensuring that either all operations successfully complete or none are performed at all

Q4.2: How does the Two-Phase Commit (2PC) protocol work to achieve atomic commit across multiple nodes? What's the difference with Three-Phase Commit (3PC) protocol?

The Two-Phase Commit (2PC) protocol achieves atomicity through a coordinator and multiple participants

1. Prepare Phase: The coordinator sends a "Prepare" request to all participants
Each participant checks if it can complete the transaction and responds with "Yes" (prepared) or "No" (abort)

2. Commit Phase: If all participants respond with "Yes," the coordinator sends a "Commit" command
If any participant responds with "No" or fails to respond (timing out), the coordinator sends an "Abort" command to roll back changes across all nodes

Three-Phase Commit (3PC) is an enhanced version of 2PC designed to prevent blocking, where participants wait indefinitely if a coordinator fails

The differences are:
* Extra Phase: 3PC adds a Pre-Commit phase between Prepare and Commit
After receiving "Yes" from everyone, the coordinator sends a "Pre-Commit" message to signal the transaction is likely to proceed

* Timeout Mechanism: 3PC allows participants to autonomously decide to commit or abort if they do not receive a final message from the coordinator within a certain timeframe
This reduces the risk of hanging transactions in unreliable networks

Q4.3: What is the difference between Choreography-based and Orchestration-based Saga implementations?

A Saga is a sequence of local transactions where failures trigger compensating transactions to undo previous changes

* Choreography-based: There is no central coordinator
Each service produces and listens to events; when a local transaction succeeds, it emits an event that triggers the next service in the chain
It is best for simple workflows and teams that require loose coupling, but it can be difficult to monitor as the system grows

* Orchestration-based: A centralized coordinator (orchestrator) manages all transactions and compensations
The orchestrator tells each participant which local transaction to execute
This is preferred for complex workflows with many steps because it provides clear visibility and centralized logic, though the orchestrator can become a performance bottleneck

Q4.4: Why is Correlation ID a good practice in distributed transactions, and how does it help with debugging and tracing?

A Correlation ID (often referred to in the sources as a Saga ID or Session ID) is a unique identifier attached to every message or operation within a single distributed transaction

It is considered a good practice for several reasons:

* Tracing and Monitoring: It allows developers to record when a specific transaction started and ended across multiple services, making it possible to track the entire lifecycle of a complex request

* Debugging: When an error occurs, the ID enables developers to filter logs across different service boundaries to see exactly where a failure happened

* Idempotency and Duplicate Handling: In choreography-based sagas, services can use the ID to identify and discard duplicate messages that may arrive due to network retries, ensuring a transaction is not processed more than once

# ACID Transactions

1. What does each component of ACID (Atomicity, Consistency, Isolation, Durability) ensure in a transaction?

The ACID properties ensure database integrity, correctness, and reliability, even during failures

* **Atomicity**: Ensures a transaction is an all-or-nothing operation. If any part of it fails, the entire transaction is rolled back, and no partial changes are applied
* **Consistency**: Guarantees the database moves from one valid state to another, following all defined rules and constraints, such as primary and foreign keys
* **Isolation**: Ensures that concurrently executing transactions do not interfere with each other. Each transaction should appear as if it is running in a serial, one-after-another order
* **Durability**: Provides a promise that once a transaction is committed, its changes are permanently saved in non-volatile memory and will not be lost, even in the event of a system crash or power failure



2. How does atomicity guarantee that a transaction is treated as an indivisible unit?

Atomicity guarantees that a transaction is treated as an indivisible unit by providing a mechanism to abort and rollback.
By treating the sequence as a single unit, the system can discard all writes from an incomplete transaction, restoring the database to its original state so that no partial, corrupting updates remain



3. What are the key differences between single-object and multi-object transactions, and why do these differences matter in system design?

While multi-object transactions simplify the programming model, they are difficult to implement across partitions in distributed systems.
Consequently, some high-performance or high-availability datastores have abandoned multi-object transactions because they can hinder scalability

* **Single-Object Transactions**: These focus on providing atomicity and isolation for changes to a single record or document, such as a 20KB JSON file or a key-value pair
This prevents issues like reading a partially updated value if a write is interrupted
* **Multi-Object Transactions**: These group multiple operations on multiple rows or records into a single unit
They are essential for keeping different pieces of data in sync, such as updating a record and its corresponding secondary index or maintaining foreign key relationships across tables



4. List and explain the various isolation levels available in DBMS and their impact on concurrent transactions.

Isolation levels define how transactions interact

* **Read Uncommitted**: The lowest level, where transactions can see uncommitted changes from others.
It offers the highest performance but risks significant data inconsistency
* **Read Committed**: The default in many databases, ensuring transactions only see committed data.
This prevents "dirty" reads and writes but still allows for inconsistencies during long-running transactions
* **Repeatable Read / Snapshot Isolation**: Ensures a transaction sees a consistent snapshot of the database as it existed at the start of the transaction.
Readers do not block writers, and vice versa
* **Serializable**: The strongest isolation level, guaranteeing that the result of parallel transactions is the same as if they had executed one at a time.
While it provides the highest consistency, it carries a heavy performance penalty
5. How do different isolation levels affect the occurrence of issues such as dirty reads, non-repeatable reads, and phantom reads?
* **Dirty Reads**: Occur when a transaction reads uncommitted data that might later be rolled back.
These are prevented by Read Committed and all higher levels
* **Non-repeatable Reads**: Occur when reading the same record twice in one transaction yields different results because another transaction modified and committed it in between.
These are prevented by Repeatable Read and Serializable levels
* **Phantom Reads**: Occur when a search query returns a different set of rows because another transaction added or deleted rows matching the condition.
These are strictly prevented by the Serializable level



6. In a distributed system, what trade-offs might you face when choosing a higher isolation level versus a lower one?

In distributed environments, choosing an isolation level involves balancing correctness against performance and availability

* **Higher Isolation (e.g., Serializability):** Provides strong guarantees against race conditions and data corruption but significantly increases latency and reduces throughput.
It often requires complex locking mechanisms or "optimistic" checks that can lead to frequent transaction aborts and retries
* **Lower Isolation (e.g., Read Committed)**: Offers better performance and scalability but forces application developers to handle complex concurrency bugs manually, such as lost updates or write skews
* **The CAP Constraint**: Distributed systems also face the CAP Theorem trade-off: in the presence of a network partition, a system must choose between Consistency (C) and Availability (A).

Prioritizing high isolation/consistency often means the system must reject requests (sacrificing availability) if it cannot guarantee the data is up-to-date across all nodes



# Conceptual Understanding (Basics)

1. What do each of the three letters in CAP (Consistency, Availability, Partition Tolerance) stand for, and what does each term mean?
* **Consistency (C):** Every read operation receives the most recent successful write or an error.
It ensures all nodes in a cluster have the same data visible at the same time, providing a uniform view for all users
* **Availability (A):** The system remains operational and returns a response for every request (read or write) in a reasonable amount of time, even if some nodes are down or the data is not the most recent
* **Partition Tolerance (P):** The system continues to function correctly despite communication breaks or network failures that split the nodes into isolated groups (partitions)



2. In your own words, state the CAP theorem. What does it assert about the ability of a distributed system to provide consistency, availability, and partition tolerance simultaneously?

the CAP theorem states that in a distributed data system, it is impossible to guarantee consistency, availability, and partition tolerance all at once.
If a network failure occurs, the system is forced to choose between maintaining a perfectly synchronized state (Consistency) or remaining responsive to users (Availability)

3. According to the CAP theorem, what trade-off must a system make when a network partition occurs?

When a network partition occurs, a system must make a critical choice:

* **Prioritize Consistency (CP):** The system will reject or delay requests if it cannot confirm the latest state with other nodes to avoid serving outdated data
This sacrifices availability
* **Prioritize Availability (AP):** The system will continue to serve requests using the data available on local nodes, even if it might be stale
This sacrifices consistency until the partition is healed and nodes can resynchronize
4. Why is Partition Tolerance often considered mandatory in real distributed systems?

In real-world distributed systems, network instability is inevitable, because designers cannot guarantee that the network will never fail, they must build systems that are partition tolerant to ensure they don't completely crash when communication issues arise.
Therefore, the practical design choice is usually between Consistency and Availability (CP vs. AP) during those inevitable failure periods

5. The “choose any two” phrasing of CAP is sometimes seen as oversimplified. Why?

The "two out of three" concept is often seen as limited for several reasons:

* **Partitions are Rare:** Designers only need to sacrifice consistency or availability when a partition actually occurs; under normal operating conditions, a system can provide both
* **The Latency Factor:** CAP ignores latency. A system could technically be "available" but take an impractically long time to respond
The PACELC theorem extends CAP by noting that even without a partition, there is a constant trade-off between Latency and Consistency
* **Tunable Consistency:** Modern databases like Cassandra or CockroachDB allow for "tunable" consistency, where the balance between these properties can be adjusted on a case-by-case basis rather than being a rigid, permanent choice



# Trade-Offs and Real-World Design Considerations

1. If a distributed system guarantees consistency and availability, what happens during a network partition?

In a distributed system, a truly Consistent and Available (CA) system is considered non-realizable because network partitions (P) are an inevitable fact of life. If a partition occurs, a CA system is forced into a binary choice:

* **Sacrifice Consistency:** The system could remain available by serving old, potentially stale data to users
* **Sacrifice Availability:** The system could maintain consistency by refusing to process requests until the nodes can communicate again, effectively becoming unavailable

Essentially, a CA system can only exist in a monolithic architecture where all data is on a single node, as there are no network links to fail

2. Give an example of a real-world CP system. Why does it choose consistency over availability?

Examples: MongoDB, Redis, Apache HBase, and CockroachDB
Why Consistency? These systems are chosen for applications where the strict accuracy of data is more important than 100% uptime.

In fields like banking, stock markets, and ticket booking, serving inconsistent or stale data could lead to major errors, such as a user booking a seat that has already been taken or seeing an incorrect financial balance.
In such scenarios, it is better for the system to show an error message and remain unavailable than to process a transaction based on incorrect information

3. Give an example of a real-world AP system. Why does it prioritize availability over consistency?

Examples: Amazon DynamoDB, Cassandra, CouchDB, and platforms like Facebook, Instagram, and YouTube
Why Availability? High availability is prioritized for services where a downtime would cause significant financial loss or drive users away.

For a social media site or a media streaming service, minor inconsistencies—such as a slight delay in updating a view count or a comment appearing a few seconds late—are acceptable as long as the service remains responsive and functional
These systems often use eventual consistency to sync data once the network partition is healed

4. How does a CP system differ from an AP system during a partition in terms of user experience?

The user experience (UX) varies drastically depending on whether a system is CP or AP:

* **CP System UX:** The user may experience blocked requests, delays, or error messages.
While frustrating, this ensures the user never interacts with "wrong" data, which is critical for high-stakes tasks
* **AP System UX:** The user experiences a highly responsive system that always provides a result. However, the data returned may be stale or "out of sync"
For example, a user might see a subscriber count that was accurate five minutes ago but doesn't reflect a very recent update
5. What factors should you consider when deciding between CP and AP for a distributed application?

When deciding between CP and AP, designers should consider the following:

* Data Integrity Requirements: If the application requires every user to see the exact same "most recent" write immediately (linearizability), consistency must be the priority
* **Impact of Downtime:** For revenue-critical services, the cost of being offline (unavailability) often outweighs the cost of temporary data inconsistency
* **Latency vs. Correctness:** Consistent systems often have higher write latency because they must coordinate across multiple nodes before confirming a success.
If speed is the primary requirement, an AP model with lower latency is usually preferred
* **Developer Complexity:** Choosing AP shifts the burden to the application developer, who must write complex code to handle data conflicts and eventual consistency.
CP systems offer a simpler contract where the most recent write is always visible to subsequent readers



# Practical Application Scenarios

1. Online banking system — prioritize consistency or availability under a partition?

Online Banking System: **Prioritize Consistency.**
In an online banking system, consistency must be prioritized (CP) over availability.

Financial transactions require absolute data integrity. If the system cannot confirm the most recent state with other nodes, it is better to reject or block the transaction and show an error message than to risk an inconsistent update

2. Social media feed — is it better to show stale data or disable the feed during a partition?

Social Media Feed: **Show Stale Data.**
For a social media feed, availability is prioritized (AP) over strict consistency.

Platforms like Facebook or Twitter prioritize a seamless user experience where the service remains responsive. Minor inconsistencies—such as a post appearing a few seconds late or a "like" count being slightly outdated—are considered acceptable trade-offs for high uptime

3. E-commerce shopping cart replication under partition — allow updates or block them?

E-commerce Shopping Cart: **Allow Updates.**
E-commerce shopping carts are frequently designed to allow updates (AP) during a partition using eventual consistency.

Large retailers like Amazon prioritize "always-on" availability to avoid losing sales. It is more profitable to let a user add an item to their cart and resolve any conflicts later than to block the purchase.

4. Configuration service scenario — continue with stale config or block usage until updated?

Configuration Service: **Block Usage Until Updated.**
In a configuration service scenario, the choice depends on the criticality of the configuration, but it typically leans toward prioritizing consistency (CP).

Configuration data often acts as "mission-critical" metadata that controls how other parts of a system behave. Using a stale configuration could cause a distributed system to perform incorrect logic, leading to widespread failures.



# Advanced Topics: PACELC and Dynamic Adjustments

1. What is the PACELC theorem, and how does it extend CAP?

The PACELC theorem extends CAP by addressing a critical oversight: how a system behaves during normal operating conditions when no network failure is present
The acronym is broken down as follows:

* **P (Partition):** If a network partition occurs...
* **A (Availability) / C (Consistency):** ...should the system favor availability or consistency? (This is the standard CAP trade-off)
* **E (Else):** Otherwise, in the absence of a partition...
* **L (Latency) / C (Consistency):** ...should the system favor low latency or strong consistency?

While CAP only forces a decision during failures, PACELC recognizes that designers must make a second, independent decision for daily performance

2. What is the latency vs. consistency trade-off in PACELC?

PACELC highlights that even when a network is healthy, speed and accuracy are in opposition

* **Consistency (C):** Strong consistency requires nodes to coordinate with one another—often waiting for multiple replicas to confirm a write—to ensure that every reader sees the most recent data. This coordination adds latency to the request
* **Latency (L):** To achieve the lowest possible latency, a system may allow "stale" or inconsistent reads, where a response is served immediately from the nearest node without checking if a more recent update exists elsewhere
3. What is tunable consistency, and how does it help adjust CAP trade-offs?

Tunable consistency is a model that gives developers the flexibility to adjust these trade-offs on a per-operation basis rather than being locked into a single system-wide preference. Apache Cassandra is a primary example of this, allowing users to select a Consistency Level (CL) for each request

* **Write/Read CL=ONE:** Only one node needs to acknowledge the request. This provides the highest availability and lowest latency but risks serving inconsistent data
* **Write/Read CL=QUORUM:** A majority of nodes must acknowledge the operation. This offers a balance between reliability and speed
* **Write/Read CL=ALL:** All replicas must confirm the operation. This ensures maximum consistency but can significantly slow down the system and makes it highly vulnerable to a single node failure
4. How might a system dynamically change CAP preferences when network conditions change?

Systems can dynamically shift their preferences based on changing network conditions or specific request requirements:

* **Bounded Staleness:** Some systems, like CockroachDB, offer "bounded staleness reads"
This allows the database to dynamically choose a historical timestamp that can be served locally from a replica without blocking on conflicting transactions, providing high availability and low latency when needed, while still guaranteeing that data is not "too old"
* **Automatic Mode Switching:** During a partition, a system might operate in a "degraded" mode (e.g., an AP system serving stale data)
Once the partition heals, the system can dynamically trigger synchronization and recovery processes to reconcile data and return to a consistent state
* **Case-by-Case Selection:** A single application might use PC/EC (strong consistency) for financial transactions while using PA/EL (low latency) for non-critical user activity logs, adjusting its CAP/PACELC preferences based on the criticality of the data being handled


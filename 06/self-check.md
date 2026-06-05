# Partitioning

## Q1.1: What is data partitioning and why is it important in distributed systems?

Data partitioning is the process of splitting a large dataset into smaller, more manageable pieces called partitions.
Each partition holds a subset of the total data, which allows systems to handle massive volumes of information without needing to scan the entire dataset for every query

In distributed systems, partitioning is essential for several reasons:

* **Scalability**: It enables horizontal scaling by allowing data to be distributed across multiple servers or storage nodes rather than being limited to the capacity of a single machine

* **Performance**: By breaking data into segments, systems can access only relevant partitions, leading to faster query execution and reduced load times

* **Resource Optimization**: It helps focus processing power on specific data subsets, preventing CPUs and RAM from being overloaded by massive datasets

* **Fault Tolerance and Availability**: If one partition fails, the rest of the system remains operational, ensuring higher availability


## Q1.2: How do horizontal and vertical partitioning differ in terms of data organization and use cases?

* **Horizontal Partitioning (Row-based)**: This method divides data by rows, where each partition contains the same columns but a subset of the rows

Use Cases: It is ideal for large datasets that need to be distributed across multiple servers to balance workload and provide high availability (e.g., splitting customer records by ID ranges)

* **Vertical Partitioning (Column-based)**: This method divides data by columns, meaning each partition contains a subset of attributes for all rows

Use Cases: It is useful when specific columns are accessed more frequently than others or when different features of an entity are stored on separate machines to improve performance (e.g., separating frequently accessed user profile data from large binary objects or KYC documents)

## Q1.3: Compare range-based and hash-based partitioning strategies. When would you choose one over the other?

* Range-Based Partitioning:
Divides data based on preset ranges of a value (e.g., dates, IDs).
Best For, Range-based queries (e.g., finding transactions between two dates).
Risks, Can lead to data skew if one range (like the current year) receives significantly more traffic.


* Hash-Based Partitioning:
Uses a hash function on a key to map data to a specific partition.
Best For, Uniform data distribution and avoiding "hotspots" or data skew.
Risks, Inefficient for range or multi-key queries because adjacent keys are not stored together.

When to choose:

* Choose **Range-based** when your application frequently performs scans or lookups over ordered values, such as time-series data or alphabetical lists

* Choose **Hash-based** when you need to evenly spread a heavy workload across many nodes and you primarily perform point lookups (retrieving a specific record by its ID)


## Q1.4: What are the benefits of using a hybrid approach that combines partitioning with replication?

A hybrid approach often seen in sharded databases with replicas on each shard, combines the performance of partitioning with the reliability of replication:

* **High Availability and Fault Tolerance**: While partitioning ensures that a failure in one part of the data doesn't crash the whole system, replication ensures that even if a specific node fails, its data remains accessible via a copy

* **Read Scaling**: By having multiple replicas for each partition, a system can handle a higher volume of read requests by distributing them across the replicas, while the partitions handle the overall storage volume

* **Performance and Durability**: This combination allows for massive parallel processing (MPP) while maintaining ACID compliance and data durability

## Q1.5: In what ways can partitioning improve query performance and scalability?

* **Query Pruning**: Systems can skip partitions that do not match the query criteria (e.g., a query for 2023 sales only scans the 2023 partition), which significantly reduces I/O

* **Parallel Execution**: Massively Parallel Processing (MPP) databases can break a single query into smaller tasks executed simultaneously across many nodes, each working on its own partition

* **Localized Lookups**: In key-value stores, knowing which partition holds a specific key allows the system to target the correct node directly, avoiding a search through the entire cluster

* **Horizontal Scalability**: It allows systems to grow beyond the hardware limits of a single machine by distributing the data and the query load across an ever-expanding cluster of nodes

* **Improved Maintenance**: Maintenance tasks like backups, index rebuilding, or archiving old data (e.g., switching out a partition for archiving) can be performed on individual partitions rather than locking the entire table


# Sharding

## Q2.1: How is sharding defined, and how does it relate to horizontal partitioning?

Database sharding is an architecture pattern where a large dataset is split into smaller logical chunks (logical shards) that are then distributed across different physical machines or database nodes (physical shards).
Each shard contains the same schema but a unique subset of the total data.

Sharding is considered a special case of horizontal partitioning
because Sharding takes this a step further by placing those row-based partitions on separate database instances, providing true horizontal scalability and high availability


## Q2.2: What factors should you consider when deciding to shard a database?

* **Hardware Limits**: The database has become "too big for one machine" and the server is choking or crashing under heavy load

* **Performance Bottlenecks**: Query performance is "crawling," and standard optimizations like indexing are no longer effective because the sheer size of the data has become the bottleneck

* **Complexity and Maintenance**: Sharding requires careful planning and implementation, often leading to higher development and maintenance costs

* **Data Consistency**: Maintaining ACID properties and global consistency across multiple shards is challenging, especially for distributed transactions

* **Shard Key Selection**: Picking a well-balanced shard key is crucial; a poor choice can lead to uneven data distribution and "hotspots"

## Q2.3: What are the main alternatives to sharding for scaling a database, and when might these alternatives be preferable to implementing sharding?

* **Vertical Scaling**: Upgrading a server's CPU, RAM, or storage
This is preferable when you have not yet hit the fixed hardware limits of a single machine, as it is simpler than managing multiple servers

* **Replication**: Creating copies of the database on multiple servers to handle read-heavy workloads
This is ideal for improving read performance and high availability without splitting the dataset

* **Partitioning (Within a Single Server)**: Splitting data into smaller parts within the same server to improve query efficiency for large datasets without the overhead of multiple database instances

* **Caching**: Using Redis or Memcached to store frequently accessed data in memory, reducing direct database load

* **CDNs (Content Delivery Networks)**: Useful for scaling read-heavy content delivery and reducing direct database access

## Q2.4: What is a "hot shard" problem, and what strategies can mitigate it?

A "hot shard" occurs when a specific partition or shard bears a disproportionate load.

Mitigation strategies include:

* **Key Hashing/Salting**: Appending a random number to a key to effectively split writes for a single entity across multiple partitions

* **Uniform Partitioning**: Using hash-based or randomized partitioning to ensure a more even distribution of data

* **Virtual Sharding**: Creating more virtual shards than physical ones and dynamically mapping them to physical nodes to rebalance load

* **Throttling and Rate Limiting**: Implementing "token bucket" or "leaky bucket" algorithms to limit requests for high-traffic keys

* **Offloading with Caching**: Using Redis or CDN-based caching to absorb the read pressure from a hot shard

* **Dynamic Rebalancing**: Monitoring per-partition load and automatically redistributing traffic or data when hotspots are detected

## Q2.5: How do secondary indexes affect the performance of partitioned databases?

Secondary indexes can improve performance for specific queries but introduce several trade-offs in a partitioned environment:

* **Cross-Shard Query Latency**: If a query filters on a column that is not the partition key, the system may need to perform cross-shard queries, scanning multiple partitions simultaneously
This is significantly slower and more complex than querying a single shard

* **Maintenance Overhead**: Every write to the database requires updating the associated indexes, which can increase the maintenance overhead and latency of distributed operations

* **Data Locality**: Systems like Cassandra use "concatenated indexes" within a partition to sort data, allowing for efficient range scans only if the specific partitioning value is provided first

* **Indexing Strategy**: Effective indexing in distributed architectures requires balancing the benefits of query acceleration against the overhead of maintaining those indexes across multiple nodes

# UC 1.1: Create a New Job

## How critical is data consistency for a concrete use-case? Are we ok with short replication lag? Why?

Data consistency is important for the user experience, but strictly speaking, global instantaneous synchronization is not business-critical. We are perfectly fine with a short replication lag (e.g., milliseconds to seconds) to secondary read replicas. 

If a job is scheduled to run tomorrow or next week, it does not matter if the worker nodes or secondary geographic regions take an extra 500 milliseconds to learn about the new job.

## What type of consistency is required for a concrete use-case: Strong, Eventual or ReadAfterWrite?

Read-After-Write Consistency (also known as Session Consistency).

While the backend background workers can tolerate Eventual Consistency, the user interface cannot. When a user clicks "Create Job," they expect to immediately see that job appear in their "Upcoming Jobs" dashboard. If we used pure Eventual Consistency, the user's immediate subsequent read might hit a replica that hasn't caught up yet, making it look like their job creation failed (resulting in confusion or duplicate submissions). Read-After-Write ensures that the user always sees the results of their own updates immediately, while allowing the broader system to synchronize eventually, preserving high availability and fast write performance.

# UC 2.1: Execute a Job At a Scheduled Time

## How critical is data consistency for a concrete use-case? Are we ok with short replication lag? Why?

Data consistency here is absolutely critical. We have zero tolerance for replication lag when it comes to the execution lock. If replication lag exists between nodes or regions, Worker A might claim the job in Region 1, while Worker B in Region 2 reads a stale state (thinking the job is still "available") and also claims it. This results in the "Double Execution" problem—a critical failure in a job scheduler

## What type of consistency is required for a concrete use-case: Strong, Eventual or ReadAfterWrite?

Strong Consistency (Linearizability).

To guarantee exactly-once execution (or to strictly prevent concurrent execution), the distributed lock acquisition must act as a single source of truth globally.

If you use Strong Consistency, the first worker gets an HTTP 201 (Created), and any subsequent worker instantly gets an HTTP 409 (Conflict), guaranteeing isolation. Any fallback to Eventual or Bounded Staleness consistency in this specific use case would reintroduce race conditions.
1. Which scalability option is the primary choice for the high load systems?

horizontal scaling (scaling out) is the primary and most effective choice, focuses on adding more servers or nodes to a network to distribute the workload

2. What are the main challenges to address horizontal scalability?

Load Balancing: algorithm selection, auto-scaling rules, maintenance and health checks
data consistency: model trade-offs (strong/eventual consistency)
session management: centralized storage (Redis)
Service Discovery: fault tolerance
testing and debugging


3. Name at least 3 metrics for hardware and software metrics which you need to consider when designing a high load system?

Hardware metrics:
CPU Usage
Memory Usage
Disk I/O

Software metrics:
Response Time
Error Rates
Availability (Uptime)


4. Give examples of the domains where high load system design is required and is not required.

Required:

Social Networks: Global platforms like Facebook, MySpace, and YouTube
Video Streaming: Services such as Netflix and Disney+
Search Engines: Platforms like Google
Large-Scale E-commerce: High-load design is critical for Amazon
Financial and Payment Systems: Banking apps and payment systems


Not Required:

Small Online Stores
Static or Low-Traffic Sites


1. NFR Mapping 

Scalability: response time (avg_ms), RPS
Availability: total requests, RPS
Reliability: p95, p99, failed requests

2. Design recommendation

Considering the number of concurrent users, I would add a load balancer to equally distribute the workload by using auto-scaling rules based on CPU/memory utilization to add more nodes of the application and for DB Reads I would add a centralized cache storage like Redis to cache repetitive GET requests to reduce load from the application/DB servers. 


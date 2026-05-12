# Describe Communication Patterns Between Components

Client Apps to API Gateway: The SPA frontend communicates with the backend via HTTPS REST APIs. 
Azure Front Door intercepts these requests at the global edge, caching static assets and providing WAF (Web Application Firewall) protection before securely routing the dynamic API calls to Azure API Management (APIM).

API Gateway to Control Plane (with Entra ID Authentication): Azure API Management (APIM) acts as the centralized identity and access enforcement point. Only after successfully verifying the token, APIM acts as a reverse proxy, forwarding authenticated and rate-limited REST requests to the Job Manager (hosted in Azure Container Apps) over secure HTTPS within the Virtual Network boundary.

Control Plane to Job Store (Azure SQL): The Job Manager interacts with the Azure SQL Database. This synchronous connection handles ACID-compliant CRUD operations for User profiles and Job configurations.

Control Plane to Job Snapshot Store (Azure Cosmos DB): The Job Manager and Job Orchestrator read and write lease locks and schedule job snapshots to Azure Cosmos DB. These queries are heavily optimized for high-throughput, low-latency point reads and writes.

Job Orchestrator to Message Broker (Event Producer): When a job is ready to run or fails to lock, the Job Orchestrator asynchronously publishes event messages (e.g., ExecuteJob, JobFailed) to Azure Service Bus.

Message Broker to Execution Nodes (Event Consumer): The Job Runners (Azure Functions) and Notification Manager (Logic Apps) asynchronously consume messages from Azure Service Bus. They use queue-triggered bindings, allowing them to instantly wake up and scale out horizontally the moment a message arrives.

Execution Nodes to External Targets (Egress): The Job Runners execute the actual user-defined payloads by making outbound HTTP/HTTPS requests (which could be REST, GraphQL, or SOAP) to Target Systems & Webhooks. This represents the arbitrary workload execution.

Execution Nodes to Log Store (Telemetry Streaming): While executing, the Job Runners asynchronously stream execution logs, errors, and standard output to Azure Log Analytics using the Azure Monitor ingestion API via HTTPS, ensuring the primary databases are not bogged down by heavy write I/O.

Notification Manager to External Channels: The Notification Manager (Logic Apps) uses pre-built API connectors to asynchronously push formatted payloads to External Communication Channels (like SendGrid for Email, or Webhooks for Slack/Teams) over HTTPS when a job success/fail.


# Provide Reasoning for Your Selection

Azure Front Door: Chosen for global load balancing, low-latency caching of static SPA assets at the edge, and providing built-in Web Application Firewall (WAF) capabilities to block malicious traffic before it ever reaches our Virtual Network.

Azure API Management (APIM): Selected to enforce secure gateway patterns. It acts as the central enforcement point for rate limiting (preventing DDoS), validates Entra ID JWT tokens, and decouples the frontend from backend routing logic.

Microsoft Entra ID: Chosen for centralized Identity and Access Management (IAM). It handles secure user authentication and, crucially, enables managed identities so our internal Azure services can securely authenticate with each other without passwords.

Azure Container Apps (Job Manager & Orchestrator): Chosen for running our control plane. It is ideal for the Orchestrator's long-running, always-on clock processes. It provides the flexibility of containerization without the massive operational overhead of managing a full Kubernetes cluster (AKS).

Azure Service Bus: Selected as the enterprise-grade message broker to guarantee the Reliability NFR. It perfectly decouples the Orchestrator from the Runners, ensures at-least-once message delivery, handles load leveling during peak execution spikes, and provides dead-letter queues for poisoned jobs.

Azure Functions (Job Runners): Chosen for its serverless, event-driven compute model. It inherently satisfies the Scalability and Cost Efficiency NFRs by automatically scaling horizontally from zero to thousands of worker nodes based on the Service Bus queue length, meaning we only pay for compute when jobs are actively executing.

Azure Logic Apps (Notification Manager): Selected for its massive library of out-of-the-box API connectors. It allows us to implement complex, multi-channel notification routing (SendGrid, Slack, Teams) via a visual workflow designer rather than writing and maintaining custom integration code.

Azure SQL Database (Job Store): Provides a managed relational database perfectly suited for our User and Job entities. It guarantees the ACID compliance, strong consistency, and referential integrity required for core platform configuration data.

Azure Cosmos DB (Job Snapshots & Leases): Chosen for its guaranteed ultra-low latency (<10ms) and highly scalable NoSQL document model. It is the perfect fit for managing rapidly changing execution schedules and acquiring distributed locks across parallel orchestrator instances without locking up a relational database.

Azure Log Analytics (Execution Logs): Selected to satisfy the Observability NFR. It is a purpose-built time-series data store capable of ingesting massive volumes of high-velocity telemetry and execution streams without impacting the performance of our primary transactional databases.



# Provide Notes About Scalability Options

Frontend Edge (Azure Front Door + Static Web Apps): Azure Front Door automatically scales its global edge nodes to absorb massive traffic spikes and DDoS attacks. Static Web Apps provides seamless, globally distributed scaling for serving the frontend SPA assets.

API Gateway (Azure API Management): APIM supports auto-scaling by dynamically adding or removing Capacity Units based on active request volume, network throughput, or CPU metrics.

Control Plane (Azure Container Apps - Job Manager): Utilizing built-in KEDA (Kubernetes Event-driven Autoscaling), the Job Manager scales out automatically based on incoming HTTP concurrent request counts, ensuring API responsiveness under load.

Messaging Layer (Azure Service Bus): Using the Premium tier, Service Bus automatically scales its Messaging Units (MUs) to process high-throughput bursts without throttling when thousands of jobs are scheduled simultaneously.

Execution Engine (Azure Functions - Job Runners): Provides true event-driven auto-scaling. The runtime automatically spins up new worker instances based on the depth of the Service Bus queues (how many jobs are waiting). It can scale to thousands of concurrent runners and scale down to zero when idle to save costs.

Relational Database (Azure SQL Database): Can be configured in the "Serverless" compute tier, which automatically scales vCores up and down based on real-time workload demand, and auto-pauses during inactive periods.

NoSQL Storage (Azure Cosmos DB): By utilizing Autoscale Provisioned Throughput, Cosmos DB instantly scales Request Units (RU/s) up to a defined maximum and down to a minimum based on actual read/write traffic, guaranteeing single-digit millisecond latency even during massive execution spikes.

Telemetry & Logging (Azure Log Analytics): Inherently scalable PaaS. It automatically provisions the necessary backend compute to ingest, index, and query millions of execution log events per minute without any manual scaling configuration required.
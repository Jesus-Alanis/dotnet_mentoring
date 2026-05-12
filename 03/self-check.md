Data Layer Capabilities

1. What alternatives exist for row-based databases? What does the term NoSQL mean?

The term NoSQL stands for "Not Only SQL". Unlike the rigid, pre-defined schemas of relational databases, NoSQL databases are flexible and schema-less

Alternatives to Row-Based Databases:

. Document Databases: JSON-like documents (such as BSON or XML).
. Key-Value Stores: every item is stored as an attribute name (key) and its value, they are optimized for speed and used for caching or session management.
. Column-Oriented (or Column-Family) Databases: Instead of rows, these store data in columns, making them well-suited for big data analytics and data warehousing.
. Graph Databases: These store data as "Nodes" (entities) and "Edges" (relationships), they excel at traversing complex connections, such as social networks or fraud detection systems

2. What are the main differences between a Database, Data Warehouse, and Data Lake?

. Relational Database (OLTP)
Consistent transactional operations (e.g., order management)
Must define structure before inserting data

. Data Warehouse (Analytical/OLAP)
Large-scale historical aggregation and business intelligence (BI)
Structured for specific reporting and analysis needs

. Data Lake
Centralized repository for all raw analytics data or native format (files or binary large objects)
No pre-defined structure: Data does not need to conform to a schema upfront

3. How can you determine if your data is structured, semi-structured, or unstructured?

Structured Data: 
This data is highly organized and follows a rigid, pre-defined schema
It is typically table-based, where every record has the same set of columns and data types
If your data must be defined in terms of tables and columns before it can be stored, it is structured

Semi-Structured Data: 
This data does not fit into a strict tabular format but contains named fields and metadata that describe the information
It is often represented in formats like JSON, XML, or BSON
In semi-structured data, different items in the same collection can have different fields, and the schema can evolve gradually

Unstructured Data: 
This is data stored in its raw or native format
It includes files and binary large objects (blobs) such as images, videos, documents, HTML files, and logs
If the data is stored as a blob without a requirement to fit into an existing organizational structure, it is unstructured


Application Layer Capabilities

1. What are the main characteristics of a Service-Oriented Architecture?

. Interoperability and Reusability: Services are designed as reusable building blocks that can be combined to form various applications
. Standardized Service Contracts: Services are governed by formal description documents and Service Level Agreements (SLAs) that define how they communicate and the quality of service provided
. Loose Coupling and Abstraction: Services are self-contained components that minimize dependencies on one another; they hide their internal logic (encapsulation) and only expose necessary interfaces
. Autonomy and Discoverability: Services have control over the logic they encapsulate and are registered in a directory where they can be effectively discovered by consumers

2. Can you explain the difference between Message-Driven and Event-Driven architectures?

. Message-Driven Architecture: This style uses messages (like JSON or XML) for inter-service communication where a client makes a specific request or command to a service and may wait for a response, which can lead to synchronous-like behavior

. Event-Driven Architecture: This style centers on events, which represent a change in state within a domain.
It is highly decoupled and asynchronous; a service (producer) publishes an event to a message broker or event bus without knowing who will consume it
Other services (subscribers) act on these events independently, often leading to eventual consistency across the system

3. What are the benefits of Asynchronous Message-Based Communication in a microservices architecture?

. Resilience and Reliability: If a service fails or a network issue occurs, the messaging infrastructure can persist the message and retry delivery, preventing data loss
. Scalability: Messaging allows for higher scalability by decoupling the sender from the receiver, enabling the system to handle unpredictable spikes in traffic
. Handling Load Spikes: Message queues act as buffers, protecting downstream services from being overwhelmed by peak loads and managing "scale-out latency" while new service instances are being provisioned
. Flexibility (Open/Closed Principle): By using a publish/subscribe model, you can add new subscriber services in the future without needing to modify the original sender service
. Eventual Consistency: It provides a mechanism to reconcile changes across different domain models (e.g., updating a customer's record across both a billing and shipping service)


4. What is a Serverless Architecture and what are its advantages and disadvantages?

Serverless architecture is a cloud computing model where cloud providers automatically manage the infrastructure, scaling, and resource allocation, allowing developers to focus solely on writing code as isolated functions

Advantages:
. Automatic Scaling: The platform automatically scales resources up or down based on the incoming workload
. Cost-Efficiency: Users follow a pay-as-you-go model, paying only for the actual compute resources used during function execution rather than for idle server time
. Reduced Operational Overhead: It abstracts away server management, eliminating the need for extensive DevOps effort regarding infrastructure maintenance
. Faster Time-to-Market: Developers can iterate and deploy code quickly because they do not have to manage underlying servers

Disadvantages:
. Cold Starts: Initial function execution may experience latency while the cloud provider initializes the runtime environment
. Resource Constraints: Platforms often impose limits on memory and maximum execution time, which may not suit long-running or compute-intensive tasks
. Vendor Lock-in: Applications may become dependent on platform-specific configurations and integrations, making it difficult to migrate to a different provider
. Complex Debugging: The distributed and stateless nature of serverless functions makes troubleshooting and testing more challenging than in traditional environments


5. How does a Hybrid Architecture leverage the strengths of different architectural styles?

Hybrid architectures combine two or more architectural styles to gain the benefits of each while mitigating their individual weaknesses

For example, an event-driven microkernel architecture leverages EDA and microkernel styles to achieve the following:

. Enhanced Performance and Scalability: By using asynchronous messaging between the core system and its plug-ins, the system can run multiple plug-ins in parallel and handle more requests without being blocked by a single task
. Improved Deployability: Decoupling plug-in components through messaging allows them to be deployed independently of the core system, increasing overall system agility
. Elasticity: The addition of a message queue provides a "back pressure point," allowing the system to absorb traffic bursts more effectively
. Evolvability: It becomes much easier to add new functionality (plug-ins) without risking the stability of a monolithic deployment




Infrastructure Layer Capabilities

1. What are the key components of web infrastructure design?

Key aspects include:
. Server Configuration: Decisions on the type, quantity, and specifications of both physical and virtual servers
. Network Architecture: Designing the layout that connects servers, load balancers, and databases, including IP addressing and routing
. Load Balancing: Implementing mechanisms to distribute incoming traffic evenly across multiple servers
. Security Measures: Incorporating firewalls (such as WAFs), SSL certificates for encryption, and security protocols to protect against cyber threats
. Database Management: Selecting appropriate systems (SQL or NoSQL) and implementing replication or clustering for redundancy
. Content Delivery: Utilizing Content Delivery Networks (CDNs) to cache and serve content from geographically closer locations
. DNS Management: Managing Domain Name System settings to map user-friendly domain names to server IP addresses
. Monitoring and Analytics: Setting up tools to track infrastructure health and user interaction performance

2. What differs HTTP 3.0 from earlier versions?

HTTP/1.x and HTTP/2 rely on TCP, which can suffer from performance issues in high-latency or lossy networks 
HTTP 3.0 utilizes a new protocol called QUIC (Quick UDP Internet Connections), which sits on top of UDP

Key differences include:
. Reduced Head-of-Line Blocking: QUIC handles packet loss at the individual stream level. The loss of a single packet does not block the entire connection
. Built-in Encryption: QUIC incorporates TLS 1.3 by default, reducing latency by eliminating the need for a separate TLS handshake
. 0-RTT Connection Establishment: QUIC enables near-instant connection establishment to previously visited servers
. Connection Migration: Designed for mobile users, QUIC allows clients to change IP addresses (e.g., switching from Wi-Fi to cellular) without losing connectivity

3. What are the roles of a Load Balancer, a Reverse Proxy, and an API Gateway in a web infrastructure? How do they differ from each other?

Load Balancer, focus on distributing network traffic based on server health and load
. Traffic distribution
. Distributes requests across a server farm to ensure scalability, fault tolerance, and high availability

Reverse Proxy, acts as a shield for backend servers
. Server protection and performance
. Caches content, handles SSL termination, and masks origin server IP addresses to improve security

API Gateway, are specialized for managing API ecosystems and decoupling client complexity from the backend architecture
. API management and entry point
. Routes requests to specific microservices, translates protocols, enforces authentication, and aggregates multiple service responses


4. What is the purpose of DNS Load Balancing and how does it work?

The purpose of DNS Load Balancing is to improve application availability and performance by directing users to different IP addresses for a single domain

How it works: 
During the DNS resolution process, an authoritative nameserver holds multiple A records (IP addresses) for a domain
When a client makes a DNS query, the load balancer uses a specific algorithm—such as Round Robin (rotating IPs sequentially), Weighted (based on server capacity), or Geo-location (closest server to the user)—to return the most appropriate IP address


5. What role does a CDN play in load balancing?

A CDN plays a vital role by delivering content from distributed edge servers rather than a single origin server

Key features include:
. Global Server Load Balancing (GSLB): CDNs use DNS-based routing and Anycast to direct users to the nearest available PoP (Point of Presence), minimizing latency
. Offloading Origin Traffic: By caching static and dynamic content at the edge, CDNs significantly reduce the volume of requests that reach the origin load balancer and servers
. Traffic Spike Mitigation: CDNs act as a buffer, absorbing volumetric traffic spikes (such as those from DDoS attacks) before they reach the main infrastructure


6. What is the function of a Web Application Firewall (WAF) in a web infrastructure?

A WAF is a Layer 7 security component that protects web applications by filtering and monitoring HTTP traffic between the application and the Internet

Key features include:
. Attack Mitigation: It helps prevent common vulnerabilities such as SQL injection, Cross-Site Scripting (XSS), and cross-site forgery
. Policy Enforcement: Unlike standard firewalls, a WAF operates through customizable rules or policies to filter out malicious traffic specific to the application's logic
. Dynamic Response: During active attacks (like a DDoS), WAF policies can be quickly modified to implement rate limiting or block specific malicious traffic patterns


7. How does an API Gateway differ from a Load Balancer and a Reverse Proxy?

API Gateway can perform load balancing and proxy functions, it is fundamentally different in its scope and complexity:
. Microservices Orchestration: An API Gateway can invoke multiple microservices and combine their results into a single response for the client—a process known as API composition
. Protocol Translation: It acts as a bridge between different communication protocols (e.g., translating a client's REST call into a backend gRPC call)
. Specialized API Policies: It provides more granular control over APIs, including request/response manipulation, rate limiting, and consumer-specific API tailoring
. Data Plane Entry Point: It serves as a specialized entry point specifically for API calls, whereas a standard load balancer is often used for all types of network traffic
# The Challenge: Rate Limiter gRPC Service

Build a standalone gRPC rate limiting service that other microservices can call to check rate limits.

## Required Files From Us
1) ratelimiter.proto (see the ## 1. gRPC Service Definition section below)

## How We Will Test Your Code

```
┌────────────────────────────────────────────────────────────────┐
│                     Testing Flow                               │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│   OurTestClient.exe                YourRateLimiterService.exe  │
│        │                                     │                 │
│        │  1. ConfigureResource               │                 │
│        │  ("api/users", 100reqs, 60sec)      │                 │
│        ├────────────────────────────────────►│                 │
│        │                                     ├─► Store Config  │
│        │◄────────────────────────────────────┤                 │
│        │  success: true                      │                 │
│        │                                     │                 │
│        │  2. ConfigureResource               │                 │
│        │  ("api/orders", 10reqs, 60sec)      │                 │
│        ├────────────────────────────────────►│                 │
│        │                                     ├─► Store Config  │
│        │◄────────────────────────────────────┤                 │
│        │  success: true                      │                 │
│        │                                     │                 │
│        │  3. CheckRateLimit                  │                 │
│        │  (client: "user1", resource:        │                 │
│        │   "api/users")                      │                 │
│        ├────────────────────────────────────►│                 │
│        │                                     ├─► Check Window  │
│        │◄────────────────────────────────────┤   Update Count  │
│        │  allowed: true                      │                 │
│        │                                     │                 │
│        │  4. CheckRateLimit x100             │                 │
│        │  (rapid fire requests)              │                 │
│        ├────────────────────────────────────►│                 │
│        │                                     ├─► Hit Limit     │
│        │◄────────────────────────────────────┤                 │
│        │  allowed: false                     │                 │
│        │                                     │                 │
└────────────────────────────────────────────────────────────────┘
```

What you need to deliver:
1. YourRateLimiterService.exe (gRPC server) that listens on port 5000
2. We will test this against our implementation of OurTestClient.exe using the provided spec.

___

# Time Expectation

*   **Target:** 1-3 hours
*   **Maximum:** Please don't spend more than 3 hours on this
*   **Note:** If you run out of time, include notes about what you would add

___

# Requirements

## x1. gRPC Service Definition

x Create a `.proto` file defining your service:

```protobuf
syntax = "proto3";

option csharp_namespace = "RateLimiter.Grpc";

service RateLimiter {
    rpc CheckRateLimit(RateLimitRequest) returns (RateLimitResponse);
    rpc ConfigureResource(ConfigureResourceRequest) returns (ConfigureResourceResponse);
}

message RateLimitRequest {
    string client_id = 1;
    string resource = 2;
}

message RateLimitResponse {
    bool allowed = 1;
}

message ConfigureResourceRequest {
    string resource = 1;
    int32 max_requests = 2;
    int32 window_seconds = 3;
}

message ConfigureResourceResponse {
    bool success = 1;
}
```
___

## 2. Core Requirements

1.  **Implement Fixed Window rate limiting algorithm**     
    -   Track requests within time windows
    -   Reset counters when window expires
2.  **Support configuration per resource type**
    -   Resources must be configured via the ConfigureResource RPC before use
    -   Different resources should have different rate limits
    -   Configuration should be dynamically updatable
3.  **Thread-safe implementation**
    -   Must handle high-concurrency scenarios
    -   No race conditions

___

## Deliverables

1.  **Source Code**
    -   GitHub repository (preferred) OR
    -   Zip file with complete solution
2.  **README.md with:**
    -   How to build and run the solution
    -   Design decisions and trade-offs
    -   What you would improve with more time
    -   Any assumptions made
    -   Performance considerations

Technical Constraints

-   **Language:** C#
-   **You may use:** Any NuGet packages you find helpful
-   **Do not use:** Existing rate limiting libraries (we want to see your implementation)
-   Persistence is not required for this activity.
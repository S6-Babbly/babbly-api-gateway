# API Gateway Development Plan for

# Babbly

This document outlines the development plan for building an API Gateway for Babbly, a
Twitter-like clone.
The gateway will handle requests to multiple microservices, provide security, rate
limiting, aggregation of responses,
monitoring, and load balancing. The gateway will interface with four microservices, each
with its own database
(Cassandra or PostgreSQL), and will be responsible for routing, aggregation, and
security enforcement.

Table of Contents:

1. 1. API Gateway Endpoint Structure
2. 2. Security Implementation (JWT)
3. 3. Rate Limiting Plan
4. 4. Monitoring and Metrics (Prometheus + Grafana)
5. 5. Load Balancing Strategy (YARP)
6. 6. Aggregation Logic and Smart Calls
7. 7. Configuration and Setup

## 1. API Gateway Endpoint Structure

The API Gateway will route all incoming requests and handle the aggregation of
responses where necessary.
The following endpoints will be proxied directly from the gateway to their respective
microservices, while others will be
aggregated and optimized for frontend consumption.

### 1.1. Pass-Through Endpoints

These endpoints will be forwarded directly from the gateway to the services without
modification:

User Service (PostgreSQL)

- GET /api/users
- GET /api/users/{id}


- GET /api/users/auth0/{auth0Id}
- POST /api/users
- PUT /api/users/{id}
- DELETE /api/users/{id}

Post Service (Cassandra)

- GET /api/posts
- GET /api/posts/{id}
- GET /api/posts/user/{userId}
- GET /api/posts/popular
- POST /api/posts
- PUT /api/posts/{id}
- DELETE /api/posts/{id}

Comment Service (Cassandra)

- GET /api/comments
- GET /api/comments/{id}
- GET /api/comments/post/{postId}
- GET /api/comments/user/{userId}
- POST /api/comments
- PUT /api/comments/{id}
- DELETE /api/comments/{id}

Like Service (Cassandra)

- GET /api/likes/{id}
- GET /api/likes/post/{postId}
- GET /api/likes/user/{userId}
- GET /api/likes/post/{postId}/count
- GET /api/likes/post/{postId}/users
- GET /api/likes/user/{userId}/posts
- GET /api/likes/check
- POST /api/likes
- DELETE /api/likes/{id}
- POST /api/likes/unlike

### 1.2. Aggregated Endpoints

The following aggregated endpoints will provide combined responses for the frontend:

- /api/feed - Get all posts with like and comment counts
- /api/feed/post/{postId} - Get post by ID with likes, comments, and like count
- /api/users/{id}/profile - Get user data, posts they made, and comments they wrote
- /api/users/{id}/activity - Get posts liked by user and comments they made


- /api/me - Get profile of logged-in user using JWT

## 2. Security Implementation (JWT)

The API Gateway will use JWT tokens for authentication. The JWT token will be passed
from the frontend,
and the gateway will verify the token before allowing access to protected endpoints.
Auth0 will be used for authentication
and issuing the JWT tokens.

- JWT token will be validated in the API Gateway using middleware.
- The `auth0Id` will be extracted from the JWT token's `sub` claim.
- User info will be forwarded to the backend services via headers, if required.

## 3. Rate Limiting Plan

Rate limiting will be implemented using the `AspNetCoreRateLimit` middleware. It will
allow for rate limiting
on a per-user or per-IP basis. Below are the proposed rate limits for the endpoints:

Endpoint Limit (requests per minute)
/api/feed 60
/api/comments* 100
/api/like 30
/api/comments 20
/api/users/* 50

## 4. Monitoring and Metrics (Prometheus + Grafana)

Prometheus and Grafana will be used for monitoring API Gateway performance. Metrics
like request counts,
latency, and errors will be tracked using Prometheus. Grafana dashboards will be set up
to visualize these metrics.

- The API Gateway will expose a `/metrics` endpoint for Prometheus scraping.
- Common metrics include request count, response times, and failed requests per route.


## 5. Load Balancing Strategy (YARP)

YARP (Yet Another Reverse Proxy) will handle load balancing between microservice
instances. By using YARP,
we can distribute incoming traffic across multiple instances of a service to improve
scalability and fault tolerance.

- Multiple instances of a microservice can be defined in `appsettings.json`.
- YARP supports load balancing strategies like `RoundRobin`, `LeastRequests`, and
`Random`.

## 6. Aggregation Logic and Smart Calls

For endpoints that require aggregation, the API Gateway will fetch data from multiple
backend services,
combine the responses, and return a single optimized response to the frontend. This will
help minimize the
number of calls the frontend needs to make.

Example:

- `GET /api/feed/post/{postId}` will aggregate the following data:
- Post information
- Comments for the post
- List of users who liked the post
- Like count

## 7. Configuration and Setup

This section outlines the configuration needed to set up the API Gateway and integrate it
with the various
backend services. It will also cover the installation of necessary NuGet packages and
middleware.

Steps:

1. Install YARP (`Microsoft.ReverseProxy` package) and other necessary packages.


2. Configure routing, load balancing, and aggregation in `appsettings.json`.
3. Set up JWT authentication middleware.
4. Configure rate limiting middleware.
5. Set up Prometheus metrics endpoint.
6. Test the gateway by calling both pass-through and aggregated endpoints.



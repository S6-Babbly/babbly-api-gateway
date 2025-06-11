# Babbly API Gateway

This API Gateway serves as the central entry point for the Babbly social media platform microservices.

## Features

- **Reverse Proxy**: Routes traffic to the appropriate microservices
- **Rate Limiting**: Protects APIs from excessive traffic
- **Aggregation**: Combines data from multiple microservices into unified responses
- **Metrics**: Prometheus integration for monitoring

## Architecture

The API Gateway connects to the following microservices:
- User Service (localhost:5001)
- Post Service (localhost:5002)
- Comment Service (localhost:5003)
- Like Service (localhost:5004)

## Endpoints

### Direct Proxied Endpoints
- `/api/users/*` → User Service
- `/api/posts/*` → Post Service
- `/api/comments/*` → Comment Service
- `/api/likes/*` → Like Service

### Aggregated Endpoints
- `/api/feed` - Get the latest posts with comments and likes
- `/api/feed/{postId}` - Get details for a specific post
- `/api/profiles/id/{userId}` - Get a user profile by ID
- `/api/profiles/username/{username}` - Get a user profile by username

## Setup

### Prerequisites
- .NET 9.0 SDK

### Running the Project Locally
```bash
# Build the project
dotnet build

# Run the project
dotnet run
```

The API will be available at http://localhost:5224 by default.

### Running with Docker

To run the gateway with Docker:

```bash
# Build the Docker image
docker build -t babbly-api-gateway .

# Run the container
docker run -p 5010:8080 babbly-api-gateway
```

### Running the Complete Application with Docker Compose

To run the entire Babbly application with all microservices:

```bash
# Navigate to the root directory containing docker-compose.yml
cd ..

# Start all services
docker-compose up -d

# Check logs
docker-compose logs -f api-gateway
```

When running with Docker Compose, the API Gateway will be available at http://localhost:5010.

## CI/CD Pipeline

This project uses GitHub Actions for continuous integration and deployment:

### Automated Workflow

1. **Build & Test**: On every push and PR to `main` and `develop` branches
   - Builds the application
   - Runs all tests
   - Collects code coverage

2. **SonarCloud Analysis**: Code quality and security scanning
   - Analyzes code for bugs, vulnerabilities, and code smells
   - Enforces coding standards
   - Tracks code coverage

3. **Docker Image**: Builds and pushes to Docker Hub
   - Creates a container image with the latest code
   - Tags with branch name, commit SHA, and 'latest'
   - Pushes to Docker Hub registry for easy deployment

### Local Development with Observability

Run the complete stack with logging and monitoring:

```bash
# Start the development environment with ELK stack and Prometheus/Grafana
docker-compose up -d

# Access monitoring tools
Kibana: http://localhost:5601
Prometheus: http://localhost:9090
Grafana: http://localhost:3000
```

## Authentication with Auth0

The API Gateway integrates with Auth0 for secure authentication:

### Configuration

The gateway accepts Auth0 configuration either through environment variables or the configuration file:

```
# Environment Variables
AUTH0_DOMAIN=your-tenant.auth0.com
AUTH0_AUDIENCE=https://api.babbly.com
```

Or in `appsettings.json`:

```json
"Auth0": {
  "Domain": "your-tenant.auth0.com",
  "Audience": "https://api.babbly.com"
}
```

### Token Validation

The gateway performs token validation at two levels:

1. **Full validation**: The ASP.NET Core authentication middleware performs complete validation of JWT tokens, including signature verification.

2. **Basic validation**: A custom middleware (`TokenValidationMiddleware`) performs basic validation of tokens for routes that don't require the full auth pipeline, checking token expiration.

### User Claims Forwarding

When a user is authenticated, the gateway extracts claims from the token and forwards them to downstream services as HTTP headers:

- `X-User-Id`: The user's Auth0 ID
- `X-User-Roles`: The user's roles (comma-separated)
- `X-User-Email`: The user's email
- `X-User-Name`: The user's display name

This allows microservices to access user information without having to validate tokens themselves.

## YARP Reverse Proxy Configuration

The gateway uses YARP (Yet Another Reverse Proxy) to route requests to the appropriate microservices:

### Route Configuration

Routes are defined in `appsettings.json` under the `ReverseProxy` section:

```json
"Routes": {
  "users-route": {
    "ClusterId": "users-cluster",
    "Match": {
      "Path": "/api/users/{**remainder}"
    },
    "AuthorizationPolicy": "authenticated"
  }
}
```

Each route specifies:
- A unique route name
- The target cluster ID
- Path matching pattern
- Optional authorization policy

### Cluster Configuration

Clusters define the destination services:

```json
"Clusters": {
  "users-cluster": {
    "Destinations": {
      "user-service": {
        "Address": "http://user-service:8080"
      }
    }
  }
}
```

### Route Transforms

The gateway applies custom transforms to requests:

- `ForwardUserClaimsTransform`: Adds user claims as headers to forwarded requests

## Running with Docker

The API Gateway and its dependent services can be started using Docker Compose:

```bash
# Start all services
docker-compose up -d

# Start only the API Gateway
docker-compose up -d api-gateway
```

Environment variables can be provided in a `.env` file:

```
AUTH0_DOMAIN=your-auth0-domain.auth0.com
AUTH0_AUDIENCE=https://api.babbly.com
``` 
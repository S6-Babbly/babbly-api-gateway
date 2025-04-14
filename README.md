# Babbly API Gateway

This API Gateway serves as the central entry point for the Babbly social media platform microservices.

## Features

- **Reverse Proxy**: Routes traffic to the appropriate microservices
- **Authentication**: JWT-based authentication using Auth0
- **Rate Limiting**: Protects APIs from excessive traffic
- **Aggregation**: Combines data from multiple microservices into unified responses
- **Metrics**: Prometheus integration for monitoring

## Architecture

The API Gateway connects to the following microservices:
- User Service (localhost:5001)
- Post Service (localhost:5002)
- Comment Service (localhost:5003)
- Like Service (localhost:5004)

## Development Mode

The gateway can be run in two modes:
1. **Mock Mode**: Uses mock data without actual microservices (default)
2. **Real Mode**: Connects to actual microservices

To toggle between modes, change the `UseMockServices` flag in `appsettings.json`:

```json
"UseMockServices": true  // Set to false to use real microservices
```

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
- `/api/profiles/me` - Get the authenticated user's profile

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

When running with Docker Compose, the API Gateway will be available at http://localhost:5010 and automatically configured to use the real microservices.

## Authentication

To test authenticated endpoints, provide a valid JWT token in the Authorization header:
```
Authorization: Bearer your-token-here
```

In mock mode, authentication is simulated but you still need to provide a token to test authenticated endpoints.

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
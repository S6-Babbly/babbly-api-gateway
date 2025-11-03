# Babbly API Gateway

The central entry point for all client requests to the Babbly social media platform, routing requests to appropriate microservices using YARP (Yet Another Reverse Proxy).

## Tech Stack

- **Backend**: ASP.NET Core 9.0
- **Reverse Proxy**: YARP (Yet Another Reverse Proxy)
- **Authentication**: Auth0 JWT validation
- **Monitoring**: Prometheus metrics, Health checks

## Features

- Service routing to backend microservices
- JWT token validation via Auth0
- User claims forwarding to downstream services
- CORS configuration for frontend access
- Health checks for all backend services
- Rate limiting per endpoint
- Request/response aggregation for complex queries

## Local Development Setup

### Prerequisites

- .NET SDK 9.0 or later
- All backend microservices running (or Docker Compose)
- Auth0 account configured

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/babbly-api-gateway.git
   cd babbly-api-gateway
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Configure Auth0 settings in `appsettings.json` or via environment variables:
   ```bash
   # Using environment variables
   export AUTH0_DOMAIN=your-tenant.auth0.com
   export AUTH0_AUDIENCE=https://api.babbly.com
   ```

4. Run the gateway:
   ```bash
   dotnet run
   ```

The API Gateway will be available at `http://localhost:5010`.

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `AUTH0_DOMAIN` | Your Auth0 tenant domain | - |
| `AUTH0_AUDIENCE` | Auth0 API audience identifier | `https://api.babbly.com` |
| `ASPNETCORE_URLS` | URLs the gateway listens on | `http://0.0.0.0:8080` |

## Service Routing

The API Gateway routes requests to the following services:

| Route Pattern | Target Service | Port |
|--------------|----------------|------|
| `/api/auth/*` | Auth Service | 5001 |
| `/api/users/*` | User Service | 8081 |
| `/api/posts/*` | Post Service | 8080 |
| `/api/comments/*` | Comment Service | 8082 |
| `/api/likes/*` | Like Service | 8083 |

### Special Endpoints

- `/api/feed` - Aggregated feed (posts with comments and likes)
- `/api/profiles/id/{userId}` - User profile by ID
- `/api/profiles/username/{username}` - User profile by username
- `/health` - Gateway health check
- `/metrics` - Prometheus metrics (development only)

## Docker Support

Run with Docker Compose (recommended for local development):

```bash
# From the root of the Babbly organization
docker-compose up -d
```

This starts the API Gateway along with all backend services and infrastructure.

The gateway will be accessible at `http://localhost:5010`.

## Architecture Notes

### Request Flow

1. Client sends request to API Gateway
2. Gateway validates JWT token (if required)
3. Gateway extracts user claims from token
4. Gateway forwards request to appropriate microservice with user claims as headers
5. Microservice processes request using forwarded user context
6. Gateway returns response to client

### User Claims Forwarding

The gateway extracts these claims from JWT tokens and forwards them as HTTP headers:

- `X-User-Id`: User's Auth0 ID
- `X-User-Roles`: User's roles (comma-separated)
- `X-User-Email`: User's email address
- `X-User-Name`: User's display name

This allows microservices to access user information without validating tokens themselves.

### Health Checks

The gateway monitors backend service health and automatically routes around unhealthy instances.

Health check configuration:
- Interval: 10 seconds
- Timeout: 5 seconds
- Policy: Consecutive failures

### YARP Configuration

Routes and clusters are defined in `appsettings.json` under the `ReverseProxy` section. The configuration is loaded at startup and can be hot-reloaded during development.

## Rate Limiting

Rate limits are enforced per endpoint:

- `/api/feed*`: 60 requests/minute
- `/api/comments*`: 100 requests/minute
- `/api/likes*`: 30 requests/minute
- `/api/users*`: 50 requests/minute
- `/api/auth*`: 100 requests/minute

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

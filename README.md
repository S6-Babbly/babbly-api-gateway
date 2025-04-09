# Babbly API Gateway

This is the API Gateway for the Babbly microservices architecture.

## Development Environment

For development purposes, you can run the API Gateway with mock microservices. This allows you to work on and test the API Gateway without needing to run the actual microservices.

### Prerequisites

- Docker
- Docker Compose

### Running the API Gateway with Mock Services

1. Clone this repository:
   ```
   git clone https://github.com/your-organization/babbly-api-gateway.git
   cd babbly-api-gateway
   ```

2. Run the development docker-compose file:
   ```
   docker-compose -f docker-compose-dev.yml up -d
   ```

3. The API Gateway will be available at http://localhost:5010

### Mock Services

The development environment uses Stoplight Prism to mock the following microservices:

- Post Service (http://localhost:5000)
- User Service (http://localhost:5001)
- Comment Service (http://localhost:5002)
- Like Service (http://localhost:5003)

Each mock service will return pre-defined positive responses for all endpoints configured in the Ocelot routing.

### Health Checks

You can verify the API Gateway is running correctly by accessing the health endpoint:

```
GET http://localhost:5010/health
```

To check the status of all mock services:

```
GET http://localhost:5010/health/services
```

### Testing Specific Endpoints

You can test the API Gateway routes with tools like curl, Postman, or the VS Code REST Client. For example:

```
# Get posts
GET http://localhost:5010/api/posts

# Get a specific post
GET http://localhost:5010/api/posts/post-123456

# Get likes for a post
GET http://localhost:5010/api/likes/post/post-123456
```

### Stopping the Development Environment

To stop the development environment:

```
docker-compose -f docker-compose-dev.yml down
```

## Development and Testing

### Using Mock Services

For development and testing without requiring the real microservices, you can use the included mock services:

```bash
docker-compose -f docker-compose.dev.yml up -d
```

This will start a stand-alone Nginx container that serves mock responses for all the Babbly microservices at http://localhost:5010.

For more information about the mock services, see the [mocks/README.md](mocks/README.md) file.

## Production Environment

For production use, the API Gateway should be run with the actual microservices:

```
docker-compose up -d
```

## Architecture

The API Gateway uses Ocelot to route requests to the appropriate microservices:

- Post Service: Handles post creation, retrieval, updates, and deletion
- User Service: Manages user accounts and authentication
- Comment Service: Handles post comments
- Like Service: Manages likes on posts

## Configuration

The API Gateway routes are configured in the `ocelot.json` and `ocelot.Development.json` files. 
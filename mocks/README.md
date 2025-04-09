# Babbly API Gateway Mock Services

This directory contains the mock services used for development and testing of the Babbly API Gateway without requiring the real microservices.

## Running the Mock Services

To run the mock services:

```bash
docker-compose -f docker-compose.dev.yml up -d
```

This will start an Nginx container that serves mock responses for all the Babbly microservices at http://localhost:5010.

## Available Endpoints

The mock service implements all the endpoints of the real microservices with fixed responses:

### Health Endpoints

- `/health` - Main health check
- `/api/health/posts` - Posts service health
- `/api/health/users` - Users service health
- `/api/health/comments` - Comments service health
- `/api/health/likes` - Likes service health

### Post Service Endpoints

- `/api/posts` - List all posts
- `/api/posts/{id}` - Get a post by ID
- `/api/posts/popular` - Get popular posts
- `/api/posts/user/{userId}` - Get posts by a specific user

### User Service Endpoints

- `/api/users` - List all users
- `/api/users/{id}` - Get a user by ID

### Comment Service Endpoints

- `/api/comments` - List all comments
- `/api/comments/{id}` - Get a comment by ID
- `/api/comments/post/{postId}` - Get comments for a specific post

### Like Service Endpoints

- `/api/likes` - List all likes
- `/api/likes/post/{postId}` - Get likes for a specific post
- `/api/likes/user/{userId}` - Get likes by a specific user

## Customizing the Mock Responses

To customize the mock responses, edit the `default.conf` file in this directory. Each endpoint is defined with a specific response based on Nginx configuration.

## Stopping the Mock Services

To stop the mock services:

```bash
docker-compose -f docker-compose.dev.yml down
``` 
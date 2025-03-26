# Babbly API Gateway

API Gateway for the Babbly platform that routes requests between the frontend and microservices.

## Overview

This API Gateway uses [Ocelot](https://github.com/ThreeMammals/Ocelot) to route requests from the frontend to the appropriate microservices:

- User Service (`babbly-user-service`)
- Post Service (`babbly-post-service`)

## Getting Started

### Prerequisites

- .NET 7.0 SDK
- Docker and Docker Compose

### Local Development

1. Clone the repository:

   ```
   git clone https://github.com/S6-Babbly/babbly-api-gateway.git
   cd babbly-api-gateway
   ```

2. Run with Docker Compose:

   ```
   docker-compose up -d
   ```

3. The API Gateway will be available at http://localhost:5010

### Configuration

The API Gateway routes are defined in the Ocelot configuration files:

- `ocelot.json` - Base configuration
- `ocelot.Development.json` - Development environment configuration
- `ocelot.Production.json` - Production environment configuration

## Route Configuration

The API Gateway uses the following routing scheme:

- `/api/users/*` - Routes to the User Service
- `/api/posts/*` - Routes to the Post Service
- `/api/health/*` - Health check endpoints for all services

## Deployment

### Docker Compose

For production deployment with Docker Compose:

```bash
DOCKER_REGISTRY=your-registry TAG=your-tag ENV=prod ./scripts/deploy.sh
```

### Kubernetes

Apply the Kubernetes manifests:

```bash
kubectl apply -f k8s/
```

## CI/CD Pipeline

This repository includes a GitHub Actions workflow for CI/CD:

1. Build and test the .NET application
2. Build and push Docker image
3. Deploy to Kubernetes cluster

Required GitHub Secrets:

- `DOCKERHUB_USERNAME` - Docker Hub username
- `DOCKERHUB_TOKEN` - Docker Hub access token
- `KUBE_CONFIG` - Kubernetes configuration
#   b a b b l y - a p i - g a t e w a y  
 
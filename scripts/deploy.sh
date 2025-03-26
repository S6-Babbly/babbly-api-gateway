#!/bin/bash

# This script deploys the API Gateway to a production environment

# Environment variables
DOCKER_REGISTRY=${DOCKER_REGISTRY:-babbly}
TAG=${TAG:-latest}
ENV=${ENV:-prod}

# Display deployment info
echo "Deploying API Gateway..."
echo "Registry: $DOCKER_REGISTRY"
echo "Tag: $TAG"
echo "Environment: $ENV"

# Pull the latest image
docker pull $DOCKER_REGISTRY/babbly-api-gateway:$TAG

# Deploy using docker-compose
docker-compose -f docker-compose.yml -f docker-compose.$ENV.yml up -d --no-build api-gateway

echo "API Gateway deployed successfully!" 
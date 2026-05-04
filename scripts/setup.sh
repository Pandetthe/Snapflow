#!/bin/bash

# Exit immediately if a command exits with a non-zero status
set -e

echo "Starting Snapflow deployment..."

# Navigate to the project root directory (assuming script is in scripts/)
cd "$(dirname "$0")/.."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
  echo "Error: Docker is not running or not installed. Please start Docker and try again."
  exit 1
fi

echo "Building and starting containers..."
docker-compose -f deployment/docker-compose.yml up -d --build

echo "Deployment successful!"
echo "Web client and API are now running via Nginx reverse proxy."
echo "To view logs, run: docker-compose -f deployment/docker-compose.yml logs -f"
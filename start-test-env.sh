#!/bin/bash
# Nanuq Test Environment Startup Script
# This script starts all required services for testing Nanuq

echo "========================================"
echo "  Nanuq Test Environment"
echo "========================================"
echo ""

# Check if Docker is running
echo "Checking Docker..."
if ! docker info > /dev/null 2>&1; then
    echo "ERROR: Docker is not running. Please start Docker."
    exit 1
fi
echo "✓ Docker is running"
echo ""

# Start services
echo "Starting services..."
docker-compose -f docker-compose.test.yml up -d

if [ $? -eq 0 ]; then
    echo ""
    echo "✓ Services started successfully!"
    echo ""

    # Wait for services to be healthy
    echo "Waiting for services to be healthy (this may take 30-60 seconds)..."
    sleep 10

    echo ""
    echo "========================================"
    echo "  Service Information"
    echo "========================================"
    echo ""

    echo "Kafka:"
    echo "  Bootstrap Server: localhost:9092"
    echo "  Authentication:   None (PLAINTEXT)"
    echo ""

    echo "RabbitMQ:"
    echo "  Server URL:     localhost:5672"
    echo "  Management UI:  http://localhost:15672"
    echo "  Username: admin"
    echo "  Password: admin123"
    echo ""

    echo "Redis:"
    echo "  Server URL: localhost:6379"
    echo "  Password:   redis123"
    echo ""

    echo "Kafka UI (Optional):"
    echo "  URL: http://localhost:8090"
    echo ""

    echo "========================================"
    echo ""
    echo "To check service status:"
    echo "  docker-compose -f docker-compose.test.yml ps"
    echo ""
    echo "To view logs:"
    echo "  docker-compose -f docker-compose.test.yml logs -f"
    echo ""
    echo "To stop services:"
    echo "  docker-compose -f docker-compose.test.yml down"
    echo ""
    echo "For detailed instructions, see TEST-CREDENTIALS.md"
    echo ""
else
    echo ""
    echo "ERROR: Failed to start services"
    echo "Check Docker logs for details"
    exit 1
fi

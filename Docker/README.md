# Docker Deployment Guide

This directory contains the Docker configuration for running Nanuq with Docker Compose.

## Architecture

The application consists of two services:

1. **backend** - .NET 8.0 Web API running on port 5000 (internal)
2. **web** - Vue.js frontend served by nginx on port 80 (exposed as 8080)

```
Browser (localhost:8080)
    ↓
web (nginx:80) ──→ Serves static Vue.js files
    ↓
    └──→ Proxies /kafka, /redis, /rabbitmq, /sqlite requests
         ↓
    backend:5000 (internal Docker network)
```

## Network Flow

- **Frontend requests**: Browser → nginx → static files
- **API requests**: Browser → nginx → backend:5000 → nginx → browser
- The backend is NOT exposed to the host (only accessible via nginx proxy)
- All communication uses the internal `nanuq-network` bridge network

## Prerequisites

- Docker
- Docker Compose

## Quick Start

```bash
# Navigate to Docker directory
cd Docker/

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

## Access the Application

- **Frontend**: http://localhost:8080
- **Backend**: Not directly accessible (proxied through frontend)

## Building Images

### Frontend Image
```bash
cd src/app/nanuq-app/
docker build -t waelouf/nanuq-app:1.0.0 .
docker build -t waelouf/nanuq-app:latest .
```

### Backend Image
```bash
cd src/services/Nanuq/
docker build -t waelouf/nanuq-server:1.0.0 -f Dockerfile .
docker build -t waelouf/nanuq-server:latest -f Dockerfile .
```
 
## Configuration

### Frontend (nginx)

The frontend nginx configuration is located at `src/app/nanuq-app/nginx.conf` and handles:

- Serving Vue.js static files
- Proxying API requests to backend
- CORS headers
- Gzip compression
- Security headers
- Static asset caching

### Backend

The backend service configuration:
- Runs on port 5000 (internal only)
- Uses SQLite database at `/app/Database/Nanuq.db`
- CORS configured to allow all origins

## Debugging

### Enable Backend Port Access

Uncomment the ports section in `docker-compose.yml`:

```yaml
backend:
  # ...
  ports:
    - "5000:5000"
```

Then restart:
```bash
docker-compose down
docker-compose up -d
```

Backend API will be accessible at http://localhost:5000

### View Container Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f web
docker-compose logs -f backend
```

### Inspect nginx Configuration

```bash
docker exec -it web cat /etc/nginx/nginx.conf
```

### Test Backend Connectivity from Frontend Container

```bash
# Enter web container
docker exec -it web sh

# Test backend connection
wget -O- http://backend:5000/kafka/topic/localhost:9092
```

## Troubleshooting

### Frontend can't reach backend

1. Check both containers are on the same network:
   ```bash
   docker network inspect nanuq-network
   ```

2. Verify backend is responding:
   ```bash
   docker exec -it web wget -O- http://backend:5000
   ```

3. Check nginx logs:
   ```bash
   docker-compose logs web
   ```

### API calls fail with 404

- Ensure API endpoints start with `/kafka`, `/redis`, `/rabbitmq`, or `/sqlite`
- Check nginx proxy configuration in `src/app/nanuq-app/nginx.conf`

### CORS errors

- CORS headers are set in nginx configuration
- Backend also has CORS configured (AllowAnyOrigin)
- Check browser console for specific CORS errors

## Production Deployment

For production:

1. Build images with specific tags
2. Push to container registry (Docker Hub, etc.)
3. Update image tags in `docker-compose.yml`
4. Consider using environment-specific compose files
5. Add health checks
6. Configure proper logging and monitoring
7. Use secrets management for sensitive data
8. Consider using Docker Swarm or Kubernetes for orchestration

## Notes

- The `Docker/nginx.conf` file is deprecated and no longer used
- nginx configuration is now embedded in the frontend container
- Backend database persists in the container (consider using volumes for production)

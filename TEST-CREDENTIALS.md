# Test Environment Credentials

This document contains the credentials for the Docker test environment.

## Starting the Test Environment

```bash
docker-compose -f docker-compose.test.yml up -d
```

## Stopping the Test Environment

```bash
docker-compose -f docker-compose.test.yml down
```

## Service Credentials

### Kafka
- **Bootstrap Server**: `localhost:9092`
- **Authentication**: None (PLAINTEXT protocol)

**Note**: Kafka runs without authentication for simplified testing. You can test connection without credentials.

### RabbitMQ
- **Server URL**: `192.168.50.94:5672`
- **Username**: `admin`
- **Password**: `admin123`
- **Management UI**: http://192.168.50.94:15672
- **Virtual Host**: `/`

### Redis
- **Server URL**: `192.168.50.94:6379`
- **Username**: *(leave empty)*
- **Password**: `redis123`

## Management UIs

### RabbitMQ Management UI
- **URL**: http://192.168.50.94:15672
- **Username**: `admin`
- **Password**: `admin123`
- Features: View exchanges, queues, bindings, publish/consume messages

### Kafka UI (Optional)
- **URL**: http://192.168.50.94:8090
- Features: View topics, consumer groups, browse messages, create topics

## Testing Workflow in Nanuq

### 1. Add Kafka Server
1. Navigate to Kafka → List Servers
2. Click "Add Server"
3. **Server Details Tab**:
   - Bootstrap Server: `localhost:9092`
   - Alias: `Local Kafka`
4. Click "Save Server"
5. **Credentials Tab**: Skip (no credentials needed for Kafka)

### 2. Add RabbitMQ Server
1. Navigate to RabbitMQ → List Servers
2. Click "Add Server"
3. **Server Details Tab**:
   - Server URL: `192.168.50.94:5672`
   - Alias: `Local RabbitMQ`
4. Click "Save Server"
5. **Credentials Tab**:
   - Username: `admin`
   - Password: `admin123`
   - Click "Test Connection" to verify
   - Click "Save Credentials"

### 3. Add Redis Server
1. Navigate to Redis → List Servers
2. Click "Add Server"
3. **Server Details Tab**:
   - Server URL: `192.168.50.94:6379`
   - Alias: `Local Redis`
4. Click "Save Server"
5. **Credentials Tab**:
   - Username: *(leave empty)*
   - Password: `redis123`
   - Click "Test Connection" to verify
   - Click "Save Credentials"

## Test Operations

### Kafka
- **Create Topic**: Navigate to server → Add Topic
- **View Topics**: Should show default topics like `__consumer_offsets`
- **Delete Topic**: Click delete icon on any test topic

### RabbitMQ
- **View Exchanges**: Navigate to server → Exchanges tab (should show default exchanges: `amq.direct`, `amq.fanout`, etc.)
- **Create Exchange**: Click "Add Exchange" → Enter name, select type
- **View Queues**: Switch to Queues tab
- **Create Queue**: Click "Add Queue" → Enter name, configure options
- **Queue Details**: Click "Details" on any queue
- **Delete**: Click delete icon on exchanges/queues

### Redis
- **View Databases**: Should list databases 0-15
- **View Keys**: Navigate to database → View keys
- **Cache String**: Add a new key-value pair
- **Get Value**: Click on a key to view its value
- **Invalidate**: Delete a cached string

## Health Checks

Check if all services are healthy:
```bash
docker-compose -f docker-compose.test.yml ps
```

All services should show "healthy" status.

## Troubleshooting

### Kafka Connection Issues
- No credentials needed (PLAINTEXT mode)
- Check logs: `docker logs nanuq-kafka`
- Ensure Zookeeper is running: `docker logs nanuq-zookeeper`

### RabbitMQ Connection Issues
- Ensure Management plugin is enabled (it is by default in the image)
- Check logs: `docker logs nanuq-rabbitmq`
- Verify Management UI is accessible: http://localhost:15672

### Redis Connection Issues
- Test connection: `docker exec nanuq-redis redis-cli -a redis123 ping`
- Should return: `PONG`
- Check logs: `docker logs nanuq-redis`

## Data Persistence

All data is stored in Docker volumes:
- `zookeeper-data` - Zookeeper data
- `kafka-data` - Kafka topics and messages
- `rabbitmq-data` - RabbitMQ exchanges, queues, and messages
- `redis-data` - Redis data (AOF persistence enabled)

Data persists between container restarts. To clean up all data:
```bash
docker-compose -f docker-compose.test.yml down -v
```

## Port Summary

| Service | Port | Description |
|---------|------|-------------|
| Zookeeper | 2181 | Zookeeper client port |
| Kafka | 9092 | Kafka PLAINTEXT (no auth) |
| RabbitMQ | 5672 | AMQP protocol |
| RabbitMQ | 15672 | Management UI |
| Redis | 6379 | Redis server |
| Kafka UI | 8090 | Kafka UI (optional) |

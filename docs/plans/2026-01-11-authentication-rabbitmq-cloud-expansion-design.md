# Nanuq Authentication, RabbitMQ & Cloud Platform Expansion - Design Document

**Date:** 2026-01-11
**Version:** 2.0.0
**Status:** Approved

## Executive Summary

This design expands Nanuq's capabilities to support:
1. **Authentication** for Kafka and Redis (username/password with encrypted storage)
2. **RabbitMQ** feature parity with Kafka (queues, exchanges, operations)
3. **Cloud platforms** - Azure (Service Bus, Event Hubs), AWS, and GCP messaging services
4. **Environment-based organization** - group servers by Dev/Staging/Production environments
5. **Dashboard UI** - environment-centric view with card-based interface

The architecture maintains Nanuq's modular monolith pattern while adding new technology modules and introducing environment-based server organization.

---

## Section 1: Architecture & Data Model

### Overview
Extend Nanuq's modular monolith architecture to support authenticated connections and cloud platforms, while introducing environment-based organization. The core architectural change is adding an "Environment" abstraction that groups servers across technologies.

### New Database Schema

Three new tables added to `Nanuq.db`:

#### 1. Environments Table
Stores environment definitions (Dev, Staging, Prod, Custom)

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER PK | Auto-increment primary key |
| Name | TEXT | Environment name (unique) |
| Description | TEXT | Optional description |
| Color | TEXT | Hex color for UI (#4CAF50, etc.) |
| IsDefault | INTEGER | Boolean flag for default environment |
| CreatedAt | TEXT | Timestamp |

#### 2. ServerCredentials Table
Encrypted credential storage for all server types

| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER PK | Auto-increment primary key |
| ServerId | INTEGER | Reference to server (Kafka/Redis/etc.) |
| ServerType | TEXT | Enum: Kafka, Redis, RabbitMQ, AzureServiceBus, etc. |
| Username | TEXT | Encrypted username |
| Password | TEXT | Encrypted password |
| AdditionalConfig | TEXT | Encrypted JSON blob for API keys, connection strings |
| EncryptionKeyId | TEXT | References encryption key version |
| CreatedAt | TEXT | Timestamp |
| UpdatedAt | TEXT | Last modified timestamp |
| LastUsedAt | TEXT | Last connection timestamp |

Unique index: `(ServerId, ServerType)`

#### 3. ServerEnvironmentMapping Table (via column addition)
Maps servers to environments by adding `EnvironmentId` column to existing tables:
- `KafkaServers.EnvironmentId` (nullable, FK to Environments)
- `RedisServers.EnvironmentId` (nullable, FK to Environments)
- `RabbitMQServers.EnvironmentId` (nullable, FK to Environments)

### Encryption Strategy

**Implementation:** Use .NET's `System.Security.Cryptography.Aes` with AES-256 encryption.

**Key Management:**
- Machine-specific key derived from DPAPI (Data Protection API) on Windows
- Keychain integration on macOS/Linux
- Encryption key stored outside database in configuration file (.gitignored)

**Security Level:** Reasonable security for dev/staging environments while remaining simple to implement.

### Repository Pattern Evolution

Each technology repository accepts optional credential parameters. Maintain backward compatibility with existing parameterless methods:

```csharp
// Existing - remains unchanged
GetTopicsAsync(string bootstrapServers)

// New - with credentials support
GetTopicsAsync(string bootstrapServers, ServerCredentials credentials)
```

---

## Section 2: Authentication for Kafka & Redis

### Kafka Authentication (SASL/PLAIN)

Extend `TopicsRepository` to support SASL authentication using Confluent.Kafka's built-in support.

**Configuration Properties:**
- `SecurityProtocol` - set to `SaslPlaintext` or `SaslSsl`
- `SaslMechanism` - set to `Plain` for username/password
- `SaslUsername` and `SaslPassword` - credential fields

**New Helper Class:** `Nanuq.Kafka/KafkaConfigBuilder.cs`

```csharp
public class KafkaConfigBuilder
{
    public static AdminClientConfig BuildConfig(string bootstrapServers, ServerCredentials? credentials = null)
    {
        var config = new AdminClientConfig { BootstrapServers = bootstrapServers };

        if (credentials != null)
        {
            config.SecurityProtocol = SecurityProtocol.SaslPlaintext;
            config.SaslMechanism = SaslMechanism.Plain;
            config.SaslUsername = credentials.Username;
            config.SaslPassword = credentials.Password;
        }

        return config;
    }
}
```

### Redis Authentication

Redis authentication uses `ConfigurationOptions` properties:
- `Password` - for basic auth
- `User` - for Redis 6+ ACL usernames

**New Helper Class:** `Nanuq.Redis/RedisConfigBuilder.cs`

```csharp
public class RedisConfigBuilder
{
    public static ConfigurationOptions BuildConfig(string serverUrl, ServerCredentials? credentials = null)
    {
        var config = new ConfigurationOptions
        {
            EndPoints = { serverUrl },
            AllowAdmin = true
        };

        if (credentials != null)
        {
            config.User = credentials.Username; // Redis 6+ ACL
            config.Password = credentials.Password;
        }

        return config;
    }
}
```

### Credential Management Service

**New Project:** `Nanuq.Security`

**Components:**
- `ICredentialService` interface: `Encrypt()`, `Decrypt()`, `Store()`, `Retrieve()`
- `AesCredentialService` implementation using AES-256
- Entity Framework entities for `ServerCredentials` table
- Repository pattern for CRUD operations on credentials

### Backward Compatibility

All existing endpoints remain unchanged. New authenticated endpoints:
- `/kafka/topic/{server}/authenticated` (requires credential ID in request body)
- `/redis/server/{server}/authenticated` (requires credential ID in request body)

---

## Section 3: RabbitMQ Implementation

### Feature Parity with Kafka

Complete the existing `Nanuq.RabbitMQ` module to match Kafka's functionality.

### Core Operations

1. **List Exchanges** (already exists as `GetRabbitMQExchanges`)
2. **List Queues** - display all queues with message counts
3. **Get Queue Details** - message count, consumer count, state (running/idle)
4. **Add Exchange** - create with type (direct, topic, fanout, headers)
5. **Add Queue** - create with durability options
6. **Delete Exchange** - remove with safety checks
7. **Delete Queue** - remove with safety checks

### RabbitMQ Client Library

Use official `RabbitMQ.Client` NuGet package.

**Connection Pattern:**
```csharp
var factory = new ConnectionFactory
{
    HostName = server,
    UserName = credentials?.Username ?? "guest",
    Password = credentials?.Password ?? "guest"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
```

### New Entities

**Location:** `Nanuq.RabbitMQ/Entities/`

- `Exchange` - (Name, Type, Durable, AutoDelete)
- `Queue` - (Name, Messages, Consumers, Durable, AutoDelete)
- `QueueDetails` - (Name, Messages, Consumers, State, MemoryUsage)

### Repository Methods

**Location:** `Nanuq.RabbitMQ/Repository/RabbitMQManagerRepository.cs`

- `GetExchangesAsync(string server, ServerCredentials? credentials)`
- `GetQueuesAsync(string server, ServerCredentials? credentials)`
- `GetQueueDetailsAsync(string server, string queueName, ServerCredentials? credentials)`
- `AddExchangeAsync(AddExchangeRequest request)`
- `AddQueueAsync(AddQueueRequest request)`
- `DeleteExchangeAsync(string server, string exchangeName, ServerCredentials? credentials)`
- `DeleteQueueAsync(string server, string queueName, ServerCredentials? credentials)`

### FastEndpoints

**Location:** `Nanuq.WebApi/Endpoints/RabbitMQ/`

- `GetExchanges.cs`
- `GetQueues.cs`
- `GetQueueDetails.cs`
- `AddExchange.cs`
- `AddQueue.cs`
- `DeleteExchange.cs`
- `DeleteQueue.cs`

---

## Section 4: Cloud Platform Architecture & Azure Implementation

### Cloud Abstraction Layer

**New Project:** `Nanuq.Cloud.Common`

Shared interfaces and abstractions for cloud messaging services.

**Common Interfaces:**
```csharp
// Base interface for all cloud messaging services
public interface ICloudMessagingRepository
{
    Task<IEnumerable<MessagingEntity>> GetEntitiesAsync(CloudConnection connection);
    Task<EntityDetails> GetEntityDetailsAsync(CloudConnection connection, string entityName);
}

// Represents a queue, topic, or stream
public record MessagingEntity(string Name, string Type, long MessageCount, string Status);

// Connection info for cloud services
public record CloudConnection(string ConnectionString, ServerCredentials? Credentials);
```

### Azure Implementation

**New Project:** `Nanuq.Azure`

Support two Azure messaging services:
1. **Service Bus** - enterprise messaging (queues and topics)
2. **Event Hubs** - event streaming (Kafka-like)

#### Azure Service Bus

**NuGet:** `Azure.Messaging.ServiceBus`

**Operations:**
- List Queues and Topics
- Get message counts (active, dead-letter, scheduled)
- Send messages to queue/topic
- Peek messages (non-destructive read)
- Purge dead-letter queue

#### Azure Event Hubs

**NuGet:** `Azure.Messaging.EventHubs`

**Operations:**
- List Event Hubs in a namespace
- Get partition information
- View consumer groups
- Send events
- Read events from partitions

#### Authentication Options

Start with connection strings (simplest), design for extensibility:

**Connection String:** Stored encrypted in `ServerCredentials.AdditionalConfig` as JSON:
```json
{"ConnectionString": "Endpoint=sb://..."}
```

**Future:** Managed Identity, Service Principal:
```json
{"TenantId": "...", "ClientId": "...", "ClientSecret": "..."}
```

### Repository Structure

**Location:** `Nanuq.Azure/`

- `Repositories/ServiceBusRepository.cs` - implements `ICloudMessagingRepository`
- `Repositories/EventHubRepository.cs` - implements `ICloudMessagingRepository`
- `Entities/ServiceBusQueue.cs`, `ServiceBusTopic.cs`, `EventHub.cs`
- `Requests/SendServiceBusMessageRequest.cs`, etc.

### FastEndpoints for Azure

**Location:** `Nanuq.WebApi/Endpoints/Azure/`

- `ServiceBus/GetQueues.cs`
- `ServiceBus/GetTopics.cs`
- `ServiceBus/SendMessage.cs`
- `EventHubs/GetEventHubs.cs`
- `EventHubs/GetPartitions.cs`

### Phase 2 & 3 (AWS and GCP)

Apply same pattern after Azure is complete:

**AWS:** `Nanuq.AWS` project
- SQS, SNS, Kinesis, EventBridge
- Use AWS SDK for .NET

**GCP:** `Nanuq.GCP` project
- Pub/Sub
- Use Google.Cloud.PubSub.V1

---

## Section 5: Environment Management & Data Organization

### Environment Management Backend

**New Project:** `Nanuq.Environments`

#### Entities

```csharp
public class Environment
{
    public int Id { get; set; }
    public string Name { get; set; } // "Development", "Staging", "Production"
    public string? Description { get; set; }
    public string Color { get; set; } // Hex color: "#4CAF50", "#FF9800", "#F44336"
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ServerEnvironmentMapping
{
    public int ServerId { get; set; }
    public ServerType ServerType { get; set; } // enum: Kafka, Redis, RabbitMQ, AzureServiceBus, etc.
    public int EnvironmentId { get; set; }
    public Environment Environment { get; set; }
}
```

#### Repository Interface

```csharp
public interface IEnvironmentRepository
{
    Task<IEnumerable<Environment>> GetAllAsync();
    Task<Environment> GetByIdAsync(int id);
    Task<Environment> CreateAsync(Environment environment);
    Task<bool> UpdateAsync(Environment environment);
    Task<bool> DeleteAsync(int id);
    Task<bool> AssignServerToEnvironmentAsync(int serverId, ServerType serverType, int environmentId);
    Task<Dictionary<int, List<ServerSummary>>> GetServersByEnvironmentAsync();
}
```

### Default Environments

Seed database with three default environments on first run:
- **Development** (Green - #4CAF50)
- **Staging** (Orange - #FF9800)
- **Production** (Red - #F44336)

### Server Configuration Updates

Modify existing SQLite server tables:
- Add `EnvironmentId` column (nullable for backward compatibility)
- Foreign key relationship to Environments table

### Migration Strategy

Existing servers without environment assignment are automatically assigned to "Development" environment on first load.

### New Endpoints

**Location:** `Nanuq.WebApi/Endpoints/Environments/`

- `GetEnvironments.cs` - list all environments
- `GetEnvironmentServers.cs` - get all servers in specific environment
- `CreateEnvironment.cs` - add custom environment
- `UpdateEnvironment.cs` - modify environment details
- `DeleteEnvironment.cs` - remove environment (requires reassigning servers)
- `AssignServerToEnvironment.cs` - map server to environment

---

## Section 6: Frontend Architecture & Dashboard UI

### New Vuex Store Modules

**Location:** `src/store/`

#### 1. environments.js
Manage environments and their servers

```javascript
state: {
  environments: [],
  selectedEnvironmentId: null,
  serversByEnvironment: {} // { environmentId: [servers] }
}

actions: {
  fetchEnvironments,
  fetchServersByEnvironment,
  createEnvironment,
  assignServerToEnvironment
}
```

#### 2. credentials.js
Manage server credentials (metadata only, passwords never sent to frontend)

```javascript
state: {
  credentials: [] // metadata only
}

actions: {
  saveCredentials,
  testConnection,
  deleteCredentials
}
```

#### 3. rabbitmq.js
RabbitMQ state management (similar to kafka.js/redis.js)

```javascript
state: {
  servers: [],
  queues: [],
  exchanges: [],
  selectedServer: null
}
```

#### 4. Cloud Platform Stores (Phase 2+)
- `azure.js`
- `aws.js`
- `gcp.js`

### Dashboard Component

**New Component:** `src/components/Dashboard.vue`

Replace current home page with environment-focused dashboard.

**Layout:**
- Header with "Add Environment" button and optional environment filter
- Grid of environment cards (3 columns desktop, 1 column mobile)
- Each card shows:
  - Environment name with color-coded header
  - Quick stats: server counts by type (2 Kafka, 3 Redis, etc.)
  - "Manage Servers" button
  - Visual health indicators (green/yellow/red)

### Environment Card Component

**New Component:** `src/components/EnvironmentCard.vue`

```vue
<v-card :color="environment.color" dark>
  <v-card-title>{{ environment.name }}</v-card-title>
  <v-card-subtitle>{{ environment.description }}</v-card-subtitle>

  <v-card-text>
    <v-chip-group column>
      <v-chip v-if="kafkaCount > 0">Kafka: {{ kafkaCount }}</v-chip>
      <v-chip v-if="redisCount > 0">Redis: {{ redisCount }}</v-chip>
      <v-chip v-if="rabbitCount > 0">RabbitMQ: {{ rabbitCount }}</v-chip>
      <v-chip v-if="azureCount > 0">Azure: {{ azureCount }}</v-chip>
    </v-chip-group>
  </v-card-text>

  <v-card-actions>
    <v-btn @click="manageServers">Manage Servers</v-btn>
  </v-card-actions>
</v-card>
```

### Server Management View

**New Component:** `src/components/ServerManagement.vue`

When user clicks "Manage Servers" on environment card:
- Tabs for each technology (Kafka, Redis, RabbitMQ, Azure, AWS, GCP)
- Each tab shows servers for that technology in selected environment
- "Add Server" button opens modal with:
  - Server type selector
  - Connection details (host, port, etc.)
  - Optional credentials section (expandable)
  - Environment assignment (pre-filled, changeable)

### Credential Input Component

**New Component:** `src/components/CredentialForm.vue`

Reusable form for entering credentials:
- Username field
- Password field (masked, with show/hide toggle)
- "Test Connection" button (validates before saving)
- Connection string field (for cloud services)
- Clear indication that credentials are encrypted at rest

### Router Updates

**Location:** `src/router/index.js`

```javascript
{
  path: '/',
  name: 'Dashboard',
  component: () => import('@/components/Dashboard.vue')
},
{
  path: '/environment/:id',
  name: 'EnvironmentServers',
  component: () => import('@/components/ServerManagement.vue')
},
{
  path: '/kafka/:serverId',
  name: 'KafkaTopics',
  component: () => import('@/kafka/KafkaTopics.vue')
},
// Similar routes for Redis, RabbitMQ, cloud services
```

### Existing Component Updates

- Kafka components: add credential support to API calls
- Redis components: add credential parameter to service calls
- Update all "Add Server" modals to include environment selector

---

## Section 7: Error Handling, Security & Testing

### Error Handling Strategy

#### Backend Custom Exceptions

**Location:** `Nanuq.Common/Exceptions/`

```csharp
public class ConnectionException : Exception
public class AuthenticationException : Exception
public class EncryptionException : Exception
public class CloudServiceException : Exception
```

#### Global Error Handler

Add middleware in `Program.cs`:

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var error = context.Features.Get<IExceptionHandlerFeature>();
        var errorResponse = new
        {
            Message = error.Error.Message,
            Type = error.Error.GetType().Name,
            // Don't expose stack traces in production
        };
        await context.Response.WriteAsJsonAsync(errorResponse);
    });
});
```

#### Connection Validation

Validate connectivity before storing server configurations:
- **Kafka:** Fetch metadata with 5-second timeout
- **Redis:** Ping command
- **RabbitMQ:** Establish connection and immediately close
- **Cloud services:** List entities or verify credentials

#### Frontend Error Handling

**New Utility:** `src/utils/errorHandler.js`

- Centralized error display using Vuetify snackbars
- Specific messages for common errors (auth failed, timeout, etc.)
- Logging to console in development mode

### Security Considerations

#### Encryption Implementation

```csharp
public class AesCredentialService : ICredentialService
{
    private readonly byte[] _key; // Derived from DPAPI

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var encrypted = encryptor.TransformFinalBlock(
            Encoding.UTF8.GetBytes(plainText), 0, plainText.Length);

        // Prepend IV to encrypted data
        return Convert.ToBase64String(aes.IV.Concat(encrypted).ToArray());
    }
}
```

#### Security Measures

- Never send decrypted passwords to frontend
- Use HTTPS in production (document in deployment guide)
- Add rate limiting to authentication endpoints
- Audit log all credential access (who, when, which server)
- Add "Last Used" timestamp to credentials for staleness detection

### Testing Strategy

#### Backend Tests (xUnit)

Create test projects for each module:
- `Nanuq.Kafka.Tests` - test with TestContainers
- `Nanuq.Redis.Tests` - test with TestContainers
- `Nanuq.RabbitMQ.Tests` - test with TestContainers
- `Nanuq.Security.Tests` - test encryption/decryption, key management
- `Nanuq.Azure.Tests` - test with Azure SDK mocks or emulator
- `Nanuq.Environments.Tests` - test environment CRUD and mappings

#### Key Test Scenarios

- Connection with and without credentials
- Encryption roundtrip (encrypt -> decrypt = original)
- Environment assignment and reassignment
- Server deletion cascades (remove credentials, environment mappings)
- Invalid credentials handling
- Connection timeout scenarios

#### Frontend Tests (Vitest)

- Environment card rendering
- Credential form validation
- API client credential passing
- Store mutations for new modules
- Component integration tests for dashboard

#### Integration Tests

- End-to-end: add server with credentials -> connect -> perform operation
- Multi-environment: assign servers to different environments, verify isolation
- Migration: existing servers get assigned to default environment

---

## Section 8: Implementation Phases & Migration Strategy

### Implementation Roadmap

#### Phase 1: Foundation (Authentication & Security)

- Create `Nanuq.Security` project with encryption service
- Add `ServerCredentials` table and Entity Framework entities
- Implement `AesCredentialService` with DPAPI key derivation
- Add credential repository and endpoints
- Update Kafka and Redis repositories to accept optional credentials
- Add backend tests for encryption and credential management
- Frontend: create `CredentialForm` component and `credentials` store module

#### Phase 2: Environment Management

- Create `Nanuq.Environments` project
- Add `Environments` and `ServerEnvironmentMapping` tables
- Implement environment repository and endpoints
- Database migration: add `EnvironmentId` to existing server tables
- Seed default environments (Dev, Staging, Production)
- Backend tests for environment CRUD and server assignments
- Frontend: create Dashboard and EnvironmentCard components
- Update routing to environment-based navigation
- Frontend tests for new components

#### Phase 3: RabbitMQ Feature Completion

- Complete `RabbitMQManagerRepository` implementation
- Add RabbitMQ entities (Queue, Exchange, QueueDetails)
- Create FastEndpoints for all RabbitMQ operations
- Add credential support to RabbitMQ connections
- Backend tests with RabbitMQ TestContainer
- Frontend: create `rabbitmq.js` store module
- Build RabbitMQ UI components (similar to Kafka structure)
- Integration tests for RabbitMQ with credentials

#### Phase 4: Azure Cloud Integration

- Create `Nanuq.Cloud.Common` project with abstractions
- Create `Nanuq.Azure` project
- Implement Service Bus repository and endpoints
- Implement Event Hubs repository and endpoints
- Add Azure-specific credential handling (connection strings)
- Backend tests with Azure SDK mocks
- Frontend: create `azure.js` store module
- Build Azure UI components (Service Bus queues/topics, Event Hubs)
- Update Dashboard to show Azure server counts

#### Phase 5: AWS Cloud Integration (Future)

- Create `Nanuq.AWS` project
- Implement SQS, SNS, Kinesis, EventBridge repositories
- Similar pattern to Azure implementation

#### Phase 6: GCP Cloud Integration (Future)

- Create `Nanuq.GCP` project
- Implement Pub/Sub repository
- Similar pattern to Azure implementation

### Database Migration with DbUp

#### New Project: Nanuq.Migrations

**Structure:**
```
Nanuq.Migrations/
├── Scripts/
│   ├── 001_CreateEnvironmentsTable.sql
│   ├── 002_CreateServerCredentialsTable.sql
│   ├── 003_AddEnvironmentIdToKafkaServers.sql
│   ├── 004_AddEnvironmentIdToRedisServers.sql
│   ├── 005_AddEnvironmentIdToRabbitMQServers.sql
│   ├── 006_SeedDefaultEnvironments.sql
│   └── 007_AssignExistingServersToDevEnvironment.sql
├── MigrationRunner.cs
└── Nanuq.Migrations.csproj
```

#### NuGet Packages

```xml
<PackageReference Include="dbup-sqlite" Version="5.0.40" />
```

#### MigrationRunner.cs

```csharp
using DbUp;
using Microsoft.Extensions.Logging;

namespace Nanuq.Migrations;

public class MigrationRunner
{
    private readonly string _connectionString;
    private readonly ILogger<MigrationRunner>? _logger;

    public MigrationRunner(string connectionString, ILogger<MigrationRunner>? logger = null)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public bool Run()
    {
        _logger?.LogInformation("Starting database migration...");

        var upgrader = DeployChanges.To
            .SQLiteDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(MigrationRunner).Assembly)
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            _logger?.LogError(result.Error, "Database migration failed");
            return false;
        }

        _logger?.LogInformation("Database migration completed successfully");
        return true;
    }

    public bool IsUpgradeRequired()
    {
        var upgrader = DeployChanges.To
            .SQLiteDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(MigrationRunner).Assembly)
            .Build();

        return upgrader.IsUpgradeRequired();
    }
}
```

#### Migration Scripts

**001_CreateEnvironmentsTable.sql:**
```sql
CREATE TABLE IF NOT EXISTS Environments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    Color TEXT NOT NULL,
    IsDefault INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Environments_Name ON Environments(Name);
```

**002_CreateServerCredentialsTable.sql:**
```sql
CREATE TABLE IF NOT EXISTS ServerCredentials (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ServerId INTEGER NOT NULL,
    ServerType TEXT NOT NULL,
    Username TEXT,
    Password TEXT,
    AdditionalConfig TEXT,
    EncryptionKeyId TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastUsedAt TEXT
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_ServerCredentials_Server
    ON ServerCredentials(ServerId, ServerType);
```

**003_AddEnvironmentIdToKafkaServers.sql:**
```sql
ALTER TABLE KafkaServers ADD COLUMN EnvironmentId INTEGER;

CREATE INDEX IF NOT EXISTS IX_KafkaServers_EnvironmentId
    ON KafkaServers(EnvironmentId);
```

**004_AddEnvironmentIdToRedisServers.sql:**
```sql
ALTER TABLE RedisServers ADD COLUMN EnvironmentId INTEGER;

CREATE INDEX IF NOT EXISTS IX_RedisServers_EnvironmentId
    ON RedisServers(EnvironmentId);
```

**005_AddEnvironmentIdToRabbitMQServers.sql:**
```sql
ALTER TABLE RabbitMQServers ADD COLUMN EnvironmentId INTEGER;

CREATE INDEX IF NOT EXISTS IX_RabbitMQServers_EnvironmentId
    ON RabbitMQServers(EnvironmentId);
```

**006_SeedDefaultEnvironments.sql:**
```sql
INSERT OR IGNORE INTO Environments (Name, Description, Color, IsDefault)
VALUES
    ('Development', 'Local development environment', '#4CAF50', 1),
    ('Staging', 'Pre-production staging environment', '#FF9800', 0),
    ('Production', 'Production environment', '#F44336', 0);
```

**007_AssignExistingServersToDevEnvironment.sql:**
```sql
-- Assign existing Kafka servers to Development environment
UPDATE KafkaServers
SET EnvironmentId = (SELECT Id FROM Environments WHERE Name = 'Development')
WHERE EnvironmentId IS NULL;

-- Assign existing Redis servers to Development environment
UPDATE RedisServers
SET EnvironmentId = (SELECT Id FROM Environments WHERE Name = 'Development')
WHERE EnvironmentId IS NULL;

-- Assign existing RabbitMQ servers to Development environment
UPDATE RabbitMQServers
SET EnvironmentId = (SELECT Id FROM Environments WHERE Name = 'Development')
WHERE EnvironmentId IS NULL;
```

#### Integration with WebApi

**Update Program.cs:**
```csharp
using Nanuq.Migrations;

var builder = WebApplication.CreateBuilder(args);

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("NanuqSqliteConfigurations");

// Run migrations on startup
var migrationRunner = new MigrationRunner(
    connectionString,
    builder.Services.BuildServiceProvider().GetService<ILogger<MigrationRunner>>()
);

if (!migrationRunner.Run())
{
    throw new Exception("Database migration failed. Application cannot start.");
}

// ... rest of Program.cs
```

#### Nanuq.Migrations.csproj Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="dbup-sqlite" Version="5.0.40" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup>
    <!-- Embed all SQL scripts as resources -->
    <EmbeddedResource Include="Scripts\**\*.sql" />
  </ItemGroup>
</Project>
```

#### Testing Migrations

**Create:** `Nanuq.Migrations.Tests` project

```csharp
public class MigrationTests
{
    [Fact]
    public void AllScripts_ShouldRunSuccessfully()
    {
        // Arrange
        var connectionString = "Data Source=:memory:";
        var runner = new MigrationRunner(connectionString);

        // Act
        var result = runner.Run();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RunningMigrationsTwice_ShouldBeIdempotent()
    {
        // Arrange
        var connectionString = "Data Source=test_migration.db";
        var runner = new MigrationRunner(connectionString);

        // Act
        var firstRun = runner.Run();
        var secondRun = runner.Run();

        // Assert
        firstRun.Should().BeTrue();
        secondRun.Should().BeTrue();

        // Cleanup
        File.Delete("test_migration.db");
    }
}
```

#### Benefits of DbUp Approach

- Clear, explicit SQL that's easy to review
- Scripts are versioned and executed in order
- Built-in tracking table prevents duplicate execution
- Easy to test migrations in isolation
- Can run migrations as part of app startup or as separate console app
- Simple rollback strategy (just write new scripts)

### Backward Compatibility

- All existing API endpoints remain unchanged
- New authenticated endpoints are separate routes
- Frontend gracefully handles servers without environment assignment
- Old docker-compose files continue to work

### Deployment Updates

#### Docker

- Update `Dockerfile` to include new NuGet packages (Azure SDK, RabbitMQ.Client)
- Add environment variable for encryption key path
- Update `docker-compose.yml` with volume mount for SQLite database
- Document credential storage location

#### Kubernetes

- Update manifests with new environment variables
- Add secret for encryption key
- Update resource limits (cloud SDKs may use more memory)

### Documentation Updates

- Update README.md feature list with new capabilities
- Add SECURITY.md documenting encryption approach and limitations
- Update CLAUDE.md with new project structure
- Create MIGRATION.md guide for existing users
- Update CHANGELOG.md for version 2.0.0

### Version Strategy

**Version:** 2.0.0 (Major Release)

**Reasons for major version:**
- New database schema (breaking change for direct DB access)
- New environment-based organization model
- Significant architectural additions

---

## Appendix: Project Structure Summary

### New Projects

```
src/services/Nanuq/
├── Nanuq.Security/              # NEW - Credential encryption and management
├── Nanuq.Environments/          # NEW - Environment management
├── Nanuq.Migrations/            # NEW - DbUp database migrations
├── Nanuq.Cloud.Common/          # NEW - Cloud abstraction layer
├── Nanuq.Azure/                 # NEW - Azure Service Bus & Event Hubs
├── Nanuq.AWS/                   # FUTURE - AWS messaging services
├── Nanuq.GCP/                   # FUTURE - GCP Pub/Sub
├── Nanuq.RabbitMQ/              # ENHANCED - Complete implementation
├── Nanuq.Kafka/                 # ENHANCED - Add authentication
├── Nanuq.Redis/                 # ENHANCED - Add authentication
├── Nanuq.WebApi/                # ENHANCED - New endpoints
├── Nanuq.Common/                # ENHANCED - New enums, exceptions
├── Nanuq.EF/                    # ENHANCED - New entities
└── Nanuq.Sqlite/                # ENHANCED - Credential storage
```

### New Frontend Components

```
src/app/nanuq-app/src/
├── components/
│   ├── Dashboard.vue            # NEW - Environment dashboard
│   ├── EnvironmentCard.vue      # NEW - Environment card component
│   ├── ServerManagement.vue     # NEW - Server management view
│   └── CredentialForm.vue       # NEW - Credential input form
├── store/
│   ├── environments.js          # NEW - Environment state
│   ├── credentials.js           # NEW - Credential management
│   ├── rabbitmq.js              # NEW - RabbitMQ state
│   ├── azure.js                 # NEW - Azure state
│   ├── aws.js                   # FUTURE - AWS state
│   └── gcp.js                   # FUTURE - GCP state
├── rabbitmq/                    # NEW - RabbitMQ UI components
├── azure/                       # NEW - Azure UI components
└── utils/
    └── errorHandler.js          # NEW - Centralized error handling
```

---

**End of Design Document**

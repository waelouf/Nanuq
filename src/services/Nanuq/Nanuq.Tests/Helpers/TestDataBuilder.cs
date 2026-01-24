using Nanuq.Common.Enums;
using Nanuq.Common.Records;

namespace Nanuq.Tests.Helpers;

/// <summary>
/// Static factory methods for creating test data
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a test ServerCredential with default values
    /// </summary>
    public static ServerCredential CreateServerCredential(
        int? id = null,
        int serverId = 1,
        ServerType serverType = ServerType.Kafka,
        string? username = "testuser",
        string? password = "testpass",
        string? additionalConfig = null,
        string encryptionKeyId = "TEST1234")
    {
        return new ServerCredential
        {
            Id = id ?? 0,
            ServerId = serverId,
            ServerType = serverType.ToString(),
            Username = username,
            Password = password,
            AdditionalConfig = additionalConfig,
            EncryptionKeyId = encryptionKeyId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastUsedAt = null
        };
    }

    /// <summary>
    /// Creates a test KafkaRecord with default values
    /// </summary>
    public static KafkaRecord CreateKafkaRecord(
        string bootstrapServer = "localhost:9092",
        string alias = "test-kafka-server",
        string environment = "Development")
    {
        return new KafkaRecord(bootstrapServer, alias, environment);
    }

    /// <summary>
    /// Creates a test RedisRecord with default values
    /// </summary>
    public static RedisRecord CreateRedisRecord(
        string serverUrl = "localhost:6379",
        string alias = "test-redis-server",
        string environment = "Development")
    {
        return new RedisRecord(serverUrl, alias, environment);
    }

    /// <summary>
    /// Creates a test RabbitMQRecord with default values
    /// </summary>
    public static RabbitMQRecord CreateRabbitMQRecord(
        string serverUrl = "localhost:5672",
        string alias = "test-rabbitmq-server",
        string environment = "Development")
    {
        return new RabbitMQRecord(serverUrl, alias, environment);
    }

    /// <summary>
    /// Creates a test AwsRecord with default values
    /// </summary>
    public static AwsRecord CreateAwsRecord(
        string region = "us-east-1",
        string alias = "test-aws-server",
        string environment = "Development",
        string serviceType = "SQS")
    {
        return new AwsRecord(region, alias, environment, serviceType);
    }

    /// <summary>
    /// Creates a test AzureRecord with default values
    /// </summary>
    public static AzureRecord CreateAzureRecord(
        string alias = "test-azure-server",
        string azureNamespace = "test-namespace.servicebus.windows.net",
        string region = "East US",
        string environment = "Development",
        string serviceType = "ServiceBus")
    {
        return new AzureRecord
        {
            Alias = alias,
            Namespace = azureNamespace,
            Region = region,
            Environment = environment,
            ServiceType = serviceType
        };
    }

    /// <summary>
    /// Creates a test ActivityLog with default values
    /// </summary>
    public static ActivityLog CreateActivityLog(
        int? id = null,
        string log = "Test Activity",
        string details = "Test Details",
        int activityTypeId = 1)
    {
        return new ActivityLog
        {
            Id = id ?? 0,
            Log = log,
            Details = details,
            Timestamp = DateTime.UtcNow,
            ActivityTypeId = activityTypeId
        };
    }

    /// <summary>
    /// Creates a test ActivityType with default values
    /// </summary>
    public static ActivityType CreateActivityType(
        int? id = null,
        string name = "Test Type",
        string description = "Test Description",
        string color = "#FF0000",
        string icon = "test-icon")
    {
        return new ActivityType
        {
            Id = id ?? 0,
            Name = name,
            Description = description,
            Color = color,
            Icon = icon
        };
    }
}

using Moq;

namespace Nanuq.Tests.Helpers;

/// <summary>
/// Factory for creating mocks of external services (Redis, Kafka, RabbitMQ, AWS, Azure)
/// </summary>
public static class MockExternalServicesFactory
{
    // Note: This is a placeholder for future Phase 2 implementation
    // Will be populated with mocks for:
    // - IConnectionMultiplexer, IDatabase, IServer (StackExchange.Redis)
    // - IAdminClient, IConsumer (Confluent.Kafka)
    // - IConnectionFactory, IConnection, IModel (RabbitMQ.Client)
    // - IAmazonSQS, IAmazonSimpleNotificationService (AWS SDK)
    // - ServiceBusAdministrationClient, ServiceBusSender, ServiceBusReceiver (Azure SDK)
}

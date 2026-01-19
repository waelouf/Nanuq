using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using Nanuq.Azure.ServiceBus.Entities;
using Nanuq.Azure.ServiceBus.Requests;
using Nanuq.Common.Records;

namespace Nanuq.Azure.ServiceBus.Repository;

public interface IServiceBusRepository
{
    // Queue operations
    Task<IEnumerable<ServiceBusQueue>> GetQueuesAsync(string connectionString);
    Task<ServiceBusQueue> GetQueueDetailsAsync(string connectionString, string queueName);
    Task CreateQueueAsync(string connectionString, CreateQueueRequest request);
    Task DeleteQueueAsync(string connectionString, string queueName);
    Task SendMessageAsync(string connectionString, SendMessageRequest request);
    Task<IEnumerable<ReceivedMessage>> ReceiveMessagesAsync(string connectionString, string queueName, int maxMessages = 10);

    // Topic operations
    Task<IEnumerable<ServiceBusTopic>> GetTopicsAsync(string connectionString);
    Task<ServiceBusTopic> GetTopicDetailsAsync(string connectionString, string topicName);
    Task CreateTopicAsync(string connectionString, CreateTopicRequest request);
    Task DeleteTopicAsync(string connectionString, string topicName);
    Task PublishMessageAsync(string connectionString, PublishMessageRequest request);

    // Subscription operations
    Task<IEnumerable<ServiceBusSubscription>> GetSubscriptionsAsync(string connectionString, string topicName);
    Task CreateSubscriptionAsync(string connectionString, CreateSubscriptionRequest request);
    Task DeleteSubscriptionAsync(string connectionString, string topicName, string subscriptionName);

    // Connection test
    Task<bool> TestConnectionAsync(string connectionString);
}

public class ServiceBusRepository : IServiceBusRepository
{
    private readonly ILogger<ServiceBusRepository> logger;

    public ServiceBusRepository(ILogger<ServiceBusRepository> logger)
    {
        this.logger = logger;
    }

    // QUEUE OPERATIONS

    public async Task<IEnumerable<ServiceBusQueue>> GetQueuesAsync(string connectionString)
    {
        var adminClient = new ServiceBusAdministrationClient(connectionString);
        var queues = new List<ServiceBusQueue>();

        await foreach (var queueProperties in adminClient.GetQueuesAsync())
        {
            var runtimeProperties = await adminClient.GetQueueRuntimePropertiesAsync(queueProperties.Name);

            queues.Add(new ServiceBusQueue
            {
                Name = queueProperties.Name,
                MessageCount = runtimeProperties.Value.ActiveMessageCount,
                DeadLetterMessageCount = runtimeProperties.Value.DeadLetterMessageCount,
                ScheduledMessageCount = runtimeProperties.Value.ScheduledMessageCount,
                TransferMessageCount = runtimeProperties.Value.TransferMessageCount,
                TransferDeadLetterMessageCount = runtimeProperties.Value.TransferDeadLetterMessageCount,
                LockDuration = queueProperties.LockDuration,
                MaxSizeInMegabytes = queueProperties.MaxSizeInMegabytes,
                RequiresDuplicateDetection = queueProperties.RequiresDuplicateDetection,
                RequiresSession = queueProperties.RequiresSession,
                DefaultMessageTimeToLive = queueProperties.DefaultMessageTimeToLive,
                AutoDeleteOnIdle = queueProperties.AutoDeleteOnIdle,
                DeadLetteringOnMessageExpiration = queueProperties.DeadLetteringOnMessageExpiration,
                MaxDeliveryCount = queueProperties.MaxDeliveryCount,
                EnableBatchedOperations = queueProperties.EnableBatchedOperations,
                Status = queueProperties.Status.ToString()
            });
        }

        return queues;
    }

    public async Task<ServiceBusQueue> GetQueueDetailsAsync(string connectionString, string queueName)
    {
        var adminClient = new ServiceBusAdministrationClient(connectionString);
        var queueProperties = await adminClient.GetQueueAsync(queueName);
        var runtimeProperties = await adminClient.GetQueueRuntimePropertiesAsync(queueName);

        return new ServiceBusQueue
        {
            Name = queueProperties.Value.Name,
            MessageCount = runtimeProperties.Value.ActiveMessageCount,
            DeadLetterMessageCount = runtimeProperties.Value.DeadLetterMessageCount,
            ScheduledMessageCount = runtimeProperties.Value.ScheduledMessageCount,
            TransferMessageCount = runtimeProperties.Value.TransferMessageCount,
            TransferDeadLetterMessageCount = runtimeProperties.Value.TransferDeadLetterMessageCount,
            LockDuration = queueProperties.Value.LockDuration,
            MaxSizeInMegabytes = queueProperties.Value.MaxSizeInMegabytes,
            RequiresDuplicateDetection = queueProperties.Value.RequiresDuplicateDetection,
            RequiresSession = queueProperties.Value.RequiresSession,
            DefaultMessageTimeToLive = queueProperties.Value.DefaultMessageTimeToLive,
            AutoDeleteOnIdle = queueProperties.Value.AutoDeleteOnIdle,
            DeadLetteringOnMessageExpiration = queueProperties.Value.DeadLetteringOnMessageExpiration,
            MaxDeliveryCount = queueProperties.Value.MaxDeliveryCount,
            EnableBatchedOperations = queueProperties.Value.EnableBatchedOperations,
            Status = queueProperties.Value.Status.ToString()
        };
    }

    public async Task CreateQueueAsync(string connectionString, CreateQueueRequest request)
    {
        var adminClient = new ServiceBusAdministrationClient(connectionString);

        var options = new CreateQueueOptions(request.QueueName)
        {
            MaxSizeInMegabytes = request.MaxSizeInMegabytes,
            MaxDeliveryCount = request.MaxDeliveryCount,
            RequiresDuplicateDetection = request.RequiresDuplicateDetection,
            RequiresSession = request.RequiresSession,
            DeadLetteringOnMessageExpiration = request.DeadLetteringOnMessageExpiration
        };

        if (request.DefaultMessageTimeToLive.HasValue)
            options.DefaultMessageTimeToLive = request.DefaultMessageTimeToLive.Value;

        if (request.LockDuration.HasValue)
            options.LockDuration = request.LockDuration.Value;

        await adminClient.CreateQueueAsync(options);
        logger.LogInformation($"Created Azure Service Bus queue: {request.QueueName}");
    }

    public async Task DeleteQueueAsync(string connectionString, string queueName)
    {
        var adminClient = new ServiceBusAdministrationClient(connectionString);
        await adminClient.DeleteQueueAsync(queueName);
        logger.LogInformation($"Deleted Azure Service Bus queue: {queueName}");
    }

    public async Task SendMessageAsync(string connectionString, SendMessageRequest request)
    {
        await using var client = new ServiceBusClient(connectionString);
        var sender = client.CreateSender(request.QueueName);

        var message = new ServiceBusMessage(request.MessageBody);

        if (!string.IsNullOrEmpty(request.ContentType))
            message.ContentType = request.ContentType;

        if (request.ApplicationProperties != null)
        {
            foreach (var prop in request.ApplicationProperties)
            {
                message.ApplicationProperties[prop.Key] = prop.Value;
            }
        }

        await sender.SendMessageAsync(message);
        logger.LogInformation($"Sent message to queue: {request.QueueName}");
    }

    public async Task<IEnumerable<ReceivedMessage>> ReceiveMessagesAsync(string connectionString, string queueName, int maxMessages = 10)
    {
        await using var client = new ServiceBusClient(connectionString);
        var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock
        });

        var messages = await receiver.ReceiveMessagesAsync(maxMessages, TimeSpan.FromSeconds(5));
        var result = new List<ReceivedMessage>();

        foreach (var message in messages)
        {
            result.Add(new ReceivedMessage
            {
                MessageId = message.MessageId,
                Body = message.Body.ToString(),
                ContentType = message.ContentType ?? string.Empty,
                EnqueuedTime = message.EnqueuedTime,
                DeliveryCount = message.DeliveryCount,
                ApplicationProperties = message.ApplicationProperties.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value
                )
            });

            // Complete the message to remove it from the queue
            await receiver.CompleteMessageAsync(message);
        }

        return result;
    }

    // TOPIC OPERATIONS

    public async Task<IEnumerable<ServiceBusTopic>> GetTopicsAsync(string connectionString)
    {
        var adminClient = new ServiceBusAdministrationClient(connectionString);
        var topics = new List<ServiceBusTopic>();

        await foreach (var topicProperties in adminClient.GetTopicsAsync())
        {
            var runtimeProperties = await adminClient.GetTopicRuntimePropertiesAsync(topicProperties.Name);

            topics.Add(new ServiceBusTopic
            {
                Name = topicProperties.Name,
                SubscriptionCount = runtimeProperties.Value.SubscriptionCount,
                MaxSizeInMegabytes = topicProperties.MaxSizeInMegabytes,
                RequiresDuplicateDetection = topicProperties.RequiresDuplicateDetection,
                DefaultMessageTimeToLive = topicProperties.DefaultMessageTimeToLive,
                AutoDeleteOnIdle = topicProperties.AutoDeleteOnIdle,
                EnableBatchedOperations = topicProperties.EnableBatchedOperations,
                SupportOrdering = topicProperties.SupportOrdering,
                Status = topicProperties.Status.ToString()
            });
        }

        return topics;
    }

    public async Task<ServiceBusTopic> GetTopicDetailsAsync(string connectionString, string topicName)
    {
        var adminClient = new ServiceBusAdministrationClient(connectionString);
        var topicProperties = await adminClient.GetTopicAsync(topicName);
        var runtimeProperties = await adminClient.GetTopicRuntimePropertiesAsync(topicName);

        return new ServiceBusTopic
        {
            Name = topicProperties.Value.Name,
            SubscriptionCount = runtimeProperties.Value.SubscriptionCount,
            MaxSizeInMegabytes = topicProperties.Value.MaxSizeInMegabytes,
            RequiresDuplicateDetection = topicProperties.Value.RequiresDuplicateDetection,
            DefaultMessageTimeToLive = topicProperties.Value.DefaultMessageTimeToLive,
            AutoDeleteOnIdle = topicProperties.Value.AutoDeleteOnIdle,
            EnableBatchedOperations = topicProperties.Value.EnableBatchedOperations,
            SupportOrdering = topicProperties.Value.SupportOrdering,
            Status = topicProperties.Value.Status.ToString()
        };
    }

    public async Task CreateTopicAsync(string connectionString, CreateTopicRequest request)
    {
        var adminClient = new ServiceBusAdministrationClient(connectionString);

        var options = new CreateTopicOptions(request.TopicName)
        {
            MaxSizeInMegabytes = request.MaxSizeInMegabytes,
            RequiresDuplicateDetection = request.RequiresDuplicateDetection,
            SupportOrdering = request.SupportOrdering
        };

        if (request.DefaultMessageTimeToLive.HasValue)
            options.DefaultMessageTimeToLive = request.DefaultMessageTimeToLive.Value;

        await adminClient.CreateTopicAsync(options);
        logger.LogInformation($"Created Azure Service Bus topic: {request.TopicName}");
    }

    public async Task DeleteTopicAsync(string connectionString, string topicName)
    {
        var adminClient = new ServiceBusAdministrationClient(connectionString);
        await adminClient.DeleteTopicAsync(topicName);
        logger.LogInformation($"Deleted Azure Service Bus topic: {topicName}");
    }

    public async Task PublishMessageAsync(string connectionString, PublishMessageRequest request)
    {
        await using var client = new ServiceBusClient(connectionString);
        var sender = client.CreateSender(request.TopicName);

        var message = new ServiceBusMessage(request.MessageBody);

        if (!string.IsNullOrEmpty(request.ContentType))
            message.ContentType = request.ContentType;

        if (request.ApplicationProperties != null)
        {
            foreach (var prop in request.ApplicationProperties)
            {
                message.ApplicationProperties[prop.Key] = prop.Value;
            }
        }

        await sender.SendMessageAsync(message);
        logger.LogInformation($"Published message to topic: {request.TopicName}");
    }

    // SUBSCRIPTION OPERATIONS

    public async Task<IEnumerable<ServiceBusSubscription>> GetSubscriptionsAsync(string connectionString, string topicName)
    {
        var adminClient = new ServiceBusAdministrationClient(connectionString);
        var subscriptions = new List<ServiceBusSubscription>();

        await foreach (var subProperties in adminClient.GetSubscriptionsAsync(topicName))
        {
            var runtimeProperties = await adminClient.GetSubscriptionRuntimePropertiesAsync(topicName, subProperties.SubscriptionName);

            subscriptions.Add(new ServiceBusSubscription
            {
                TopicName = topicName,
                SubscriptionName = subProperties.SubscriptionName,
                MessageCount = runtimeProperties.Value.ActiveMessageCount,
                DeadLetterMessageCount = runtimeProperties.Value.DeadLetterMessageCount,
                LockDuration = subProperties.LockDuration,
                RequiresSession = subProperties.RequiresSession,
                DefaultMessageTimeToLive = subProperties.DefaultMessageTimeToLive,
                AutoDeleteOnIdle = subProperties.AutoDeleteOnIdle,
                DeadLetteringOnMessageExpiration = subProperties.DeadLetteringOnMessageExpiration,
                MaxDeliveryCount = subProperties.MaxDeliveryCount,
                Status = subProperties.Status.ToString()
            });
        }

        return subscriptions;
    }

    public async Task CreateSubscriptionAsync(string connectionString, CreateSubscriptionRequest request)
    {
        var adminClient = new ServiceBusAdministrationClient(connectionString);

        var options = new CreateSubscriptionOptions(request.TopicName, request.SubscriptionName)
        {
            MaxDeliveryCount = request.MaxDeliveryCount,
            RequiresSession = request.RequiresSession,
            DeadLetteringOnMessageExpiration = request.DeadLetteringOnMessageExpiration
        };

        if (request.LockDuration.HasValue)
            options.LockDuration = request.LockDuration.Value;

        await adminClient.CreateSubscriptionAsync(options);
        logger.LogInformation($"Created subscription: {request.SubscriptionName} for topic: {request.TopicName}");
    }

    public async Task DeleteSubscriptionAsync(string connectionString, string topicName, string subscriptionName)
    {
        var adminClient = new ServiceBusAdministrationClient(connectionString);
        await adminClient.DeleteSubscriptionAsync(topicName, subscriptionName);
        logger.LogInformation($"Deleted subscription: {subscriptionName} from topic: {topicName}");
    }

    // CONNECTION TEST

    public async Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            var adminClient = new ServiceBusAdministrationClient(connectionString);
            // Try to enumerate queues to test connection
            await foreach (var _ in adminClient.GetQueuesAsync())
            {
                break; // Just need to test connection, not enumerate all
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Azure Service Bus connection test failed");
            return false;
        }
    }
}

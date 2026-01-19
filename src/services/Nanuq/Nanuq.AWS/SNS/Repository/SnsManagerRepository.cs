using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Nanuq.AWS.SNS.Entities;
using Nanuq.AWS.SNS.Helpers;
using Nanuq.AWS.SNS.Interfaces;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using System.Text.Json;
using SnsCreateTopicRequest = Nanuq.AWS.SNS.Requests.CreateTopicRequest;
using SnsPublishMessageRequest = Nanuq.AWS.SNS.Requests.PublishMessageRequest;
using SnsSubscribeRequest = Nanuq.AWS.SNS.Requests.SubscribeRequest;

namespace Nanuq.AWS.SNS.Repository;

/// <summary>
/// Repository for managing SNS topics and subscriptions
/// </summary>
public class SnsManagerRepository : ISnsManagerRepository
{
    private readonly ILogger<SnsManagerRepository> logger;
    private readonly IAuditLogRepository auditLog;

    public SnsManagerRepository(
        ILogger<SnsManagerRepository> logger,
        IAuditLogRepository auditLog)
    {
        this.logger = logger;
        this.auditLog = auditLog;
    }

    public async Task<IEnumerable<SnsTopic>> GetTopicsAsync(string region, ServerCredential credential)
    {
        try
        {
            using var client = SnsConfigBuilder.BuildClient(region, credential);

            var request = new ListTopicsRequest();
            var response = await client.ListTopicsAsync(request);

            var topics = new List<SnsTopic>();

            foreach (var topic in response.Topics)
            {
                var topicName = ExtractTopicName(topic.TopicArn);

                // Get subscription count
                var subsRequest = new ListSubscriptionsByTopicRequest
                {
                    TopicArn = topic.TopicArn
                };
                var subsResponse = await client.ListSubscriptionsByTopicAsync(subsRequest);

                topics.Add(new SnsTopic(topic.TopicArn, topicName, subsResponse.Subscriptions.Count));
            }

            logger.LogInformation("Retrieved {Count} topics from region {Region}", topics.Count, region);
            return topics;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving topics from region {Region}", region);
            throw;
        }
    }

    public async Task<SnsTopicDetails> GetTopicDetailsAsync(string region, string topicArn, ServerCredential credential)
    {
        try
        {
            using var client = SnsConfigBuilder.BuildClient(region, credential);

            var request = new GetTopicAttributesRequest
            {
                TopicArn = topicArn
            };

            var response = await client.GetTopicAttributesAsync(request);
            var attrs = response.Attributes;

            var topicName = ExtractTopicName(topicArn);
            var createdAt = DateTime.UtcNow; // SNS doesn't provide created timestamp

            return new SnsTopicDetails(
                topicArn,
                topicName,
                attrs.GetValueOrDefault("DisplayName", ""),
                attrs.GetValueOrDefault("Owner", ""),
                int.Parse(attrs.GetValueOrDefault("SubscriptionsConfirmed", "0")),
                int.Parse(attrs.GetValueOrDefault("SubscriptionsPending", "0")),
                int.Parse(attrs.GetValueOrDefault("SubscriptionsDeleted", "0")),
                createdAt
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving topic details for {TopicArn}", topicArn);
            throw;
        }
    }

    public async Task<string> CreateTopicAsync(SnsCreateTopicRequest request, ServerCredential credential)
    {
        try
        {
            using var client = SnsConfigBuilder.BuildClient(request.Region, credential);

            var createRequest = new CreateTopicRequest
            {
                Name = request.TopicName
            };

            if (request.IsFifo)
            {
                createRequest.Attributes.Add("FifoTopic", "true");
                createRequest.Attributes.Add("ContentBasedDeduplication", "true");
            }

            if (!string.IsNullOrEmpty(request.DisplayName))
            {
                createRequest.Attributes.Add("DisplayName", request.DisplayName);
            }

            var response = await client.CreateTopicAsync(createRequest);

            // Audit logging
            var details = JsonSerializer.Serialize(new
            {
                topicName = request.TopicName,
                region = request.Region,
                topicArn = response.TopicArn,
                isFifo = request.IsFifo
            });

            await auditLog.Audit(
                ActivityTypeEnum.AddSNSTopic,
                $"SNS topic '{request.TopicName}' created in region '{request.Region}'",
                details);

            logger.LogInformation("Created topic {TopicName} in region {Region}", request.TopicName, request.Region);
            return response.TopicArn;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating topic {TopicName}", request.TopicName);
            throw;
        }
    }

    public async Task<bool> DeleteTopicAsync(string region, string topicArn, ServerCredential credential)
    {
        try
        {
            using var client = SnsConfigBuilder.BuildClient(region, credential);

            var topicName = ExtractTopicName(topicArn);

            // Audit log BEFORE deletion
            var details = JsonSerializer.Serialize(new
            {
                topicName,
                region,
                topicArn
            });

            await auditLog.Audit(
                ActivityTypeEnum.RemoveSNSTopic,
                $"SNS topic '{topicName}' deleted from region '{region}'",
                details);

            await client.DeleteTopicAsync(topicArn);

            logger.LogInformation("Deleted topic {TopicArn}", topicArn);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting topic {TopicArn}", topicArn);
            throw;
        }
    }

    public async Task<string> PublishMessageAsync(SnsPublishMessageRequest request, ServerCredential credential)
    {
        try
        {
            using var client = SnsConfigBuilder.BuildClient(request.Region, credential);

            var publishRequest = new PublishRequest
            {
                TopicArn = request.TopicArn,
                Message = request.Message,
                Subject = request.Subject
            };

            if (request.MessageAttributes != null)
            {
                foreach (var attr in request.MessageAttributes)
                {
                    publishRequest.MessageAttributes.Add(attr.Key, new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = attr.Value
                    });
                }
            }

            if (!string.IsNullOrEmpty(request.MessageGroupId))
            {
                publishRequest.MessageGroupId = request.MessageGroupId;
            }

            if (!string.IsNullOrEmpty(request.MessageDeduplicationId))
            {
                publishRequest.MessageDeduplicationId = request.MessageDeduplicationId;
            }

            var response = await client.PublishAsync(publishRequest);

            // Audit logging
            var details = JsonSerializer.Serialize(new
            {
                topicArn = request.TopicArn,
                messageId = response.MessageId,
                subject = request.Subject,
                messageLength = request.Message.Length
            });

            await auditLog.Audit(
                ActivityTypeEnum.PublishSNSMessage,
                $"Message published to SNS topic '{ExtractTopicName(request.TopicArn)}'",
                details);

            logger.LogInformation("Published message to topic {TopicArn}", request.TopicArn);
            return response.MessageId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing message to topic {TopicArn}", request.TopicArn);
            throw;
        }
    }

    public async Task<IEnumerable<SnsSubscription>> GetSubscriptionsAsync(string region, string topicArn, ServerCredential credential)
    {
        try
        {
            using var client = SnsConfigBuilder.BuildClient(region, credential);

            var request = new ListSubscriptionsByTopicRequest
            {
                TopicArn = topicArn
            };

            var response = await client.ListSubscriptionsByTopicAsync(request);

            var subscriptions = response.Subscriptions.Select(s => new SnsSubscription(
                s.SubscriptionArn,
                s.Protocol,
                s.Endpoint,
                s.Owner,
                s.SubscriptionArn == "PendingConfirmation" ? "PendingConfirmation" : "Confirmed"
            )).ToList();

            logger.LogInformation("Retrieved {Count} subscriptions for topic {TopicArn}", subscriptions.Count, topicArn);
            return subscriptions;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving subscriptions for topic {TopicArn}", topicArn);
            throw;
        }
    }

    public async Task<string> SubscribeAsync(SnsSubscribeRequest request, ServerCredential credential)
    {
        try
        {
            using var client = SnsConfigBuilder.BuildClient(request.Region, credential);

            var subscribeRequest = new Amazon.SimpleNotificationService.Model.SubscribeRequest
            {
                TopicArn = request.TopicArn,
                Protocol = request.Protocol,
                Endpoint = request.Endpoint
            };

            var response = await client.SubscribeAsync(subscribeRequest);

            // Audit logging
            var details = JsonSerializer.Serialize(new
            {
                topicArn = request.TopicArn,
                protocol = request.Protocol,
                endpoint = request.Endpoint,
                subscriptionArn = response.SubscriptionArn
            });

            await auditLog.Audit(
                ActivityTypeEnum.AddSNSSubscription,
                $"Subscription created for SNS topic '{ExtractTopicName(request.TopicArn)}' ({request.Protocol}: {request.Endpoint})",
                details);

            logger.LogInformation("Created subscription for topic {TopicArn}", request.TopicArn);
            return response.SubscriptionArn;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error subscribing to topic {TopicArn}", request.TopicArn);
            throw;
        }
    }

    public async Task<bool> UnsubscribeAsync(string region, string subscriptionArn, ServerCredential credential)
    {
        try
        {
            using var client = SnsConfigBuilder.BuildClient(region, credential);

            await client.UnsubscribeAsync(subscriptionArn);

            logger.LogInformation("Unsubscribed {SubscriptionArn}", subscriptionArn);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unsubscribing {SubscriptionArn}", subscriptionArn);
            throw;
        }
    }

    public async Task<bool> TestConnectionAsync(string region, ServerCredential credential)
    {
        try
        {
            using var client = SnsConfigBuilder.BuildClient(region, credential);
            await client.ListTopicsAsync(new ListTopicsRequest());
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Connection test failed for region {Region}", region);
            return false;
        }
    }

    /// <summary>
    /// Extracts topic name from topic ARN
    /// </summary>
    private string ExtractTopicName(string topicArn)
    {
        return topicArn.Split(':').Last();
    }
}

using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Nanuq.AWS.SQS.Entities;
using Nanuq.AWS.SQS.Helpers;
using Nanuq.AWS.SQS.Interfaces;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using System.Text.Json;
using SqsCreateQueueRequest = Nanuq.AWS.SQS.Requests.CreateQueueRequest;
using SqsSendMessageRequest = Nanuq.AWS.SQS.Requests.SendMessageRequest;
using SqsReceiveMessagesRequest = Nanuq.AWS.SQS.Requests.ReceiveMessagesRequest;

namespace Nanuq.AWS.SQS.Repository;

/// <summary>
/// Repository for managing SQS queues and messages
/// </summary>
public class SqsManagerRepository : ISqsManagerRepository
{
    private readonly ILogger<SqsManagerRepository> logger;
    private readonly IAuditLogRepository auditLog;

    public SqsManagerRepository(
        ILogger<SqsManagerRepository> logger,
        IAuditLogRepository auditLog)
    {
        this.logger = logger;
        this.auditLog = auditLog;
    }

    public async Task<IEnumerable<SqsQueue>> GetQueuesAsync(string region, ServerCredential credential)
    {
        try
        {
            using var client = SqsConfigBuilder.BuildClient(region, credential);

            var request = new ListQueuesRequest();
            var response = await client.ListQueuesAsync(request);

            var queues = new List<SqsQueue>();

            // Get attributes for each queue
            foreach (var queueUrl in response.QueueUrls)
            {
                var queueName = ExtractQueueName(queueUrl);
                var attributesRequest = new GetQueueAttributesRequest
                {
                    QueueUrl = queueUrl,
                    AttributeNames = new List<string> { "ApproximateNumberOfMessages" }
                };

                var attributesResponse = await client.GetQueueAttributesAsync(attributesRequest);
                var messageCount = 0;

                if (attributesResponse.Attributes.TryGetValue("ApproximateNumberOfMessages", out var countStr))
                {
                    int.TryParse(countStr, out messageCount);
                }

                queues.Add(new SqsQueue(queueName, queueUrl, messageCount));
            }

            logger.LogInformation("Retrieved {Count} queues from region {Region}", queues.Count, region);
            return queues;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving queues from region {Region}", region);
            throw;
        }
    }

    public async Task<SqsQueueDetails> GetQueueDetailsAsync(string region, string queueUrl, ServerCredential credential)
    {
        try
        {
            using var client = SqsConfigBuilder.BuildClient(region, credential);

            var request = new GetQueueAttributesRequest
            {
                QueueUrl = queueUrl,
                AttributeNames = new List<string> { "All" }
            };

            var response = await client.GetQueueAttributesAsync(request);
            var attrs = response.Attributes;

            var queueName = ExtractQueueName(queueUrl);
            var createdTimestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(attrs.GetValueOrDefault("CreatedTimestamp", "0"))).UtcDateTime;
            var lastModifiedTimestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(attrs.GetValueOrDefault("LastModifiedTimestamp", "0"))).UtcDateTime;

            return new SqsQueueDetails(
                queueName,
                queueUrl,
                int.Parse(attrs.GetValueOrDefault("ApproximateNumberOfMessages", "0")),
                int.Parse(attrs.GetValueOrDefault("ApproximateNumberOfMessagesNotVisible", "0")),
                int.Parse(attrs.GetValueOrDefault("ApproximateNumberOfMessagesDelayed", "0")),
                createdTimestamp,
                lastModifiedTimestamp,
                int.Parse(attrs.GetValueOrDefault("VisibilityTimeout", "30")),
                int.Parse(attrs.GetValueOrDefault("MessageRetentionPeriod", "345600")),
                int.Parse(attrs.GetValueOrDefault("MaximumMessageSize", "262144")),
                attrs.GetValueOrDefault("RedrivePolicy", null),
                queueName.EndsWith(".fifo")
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving queue details for {QueueUrl}", queueUrl);
            throw;
        }
    }

    public async Task<string> CreateQueueAsync(SqsCreateQueueRequest request, ServerCredential credential)
    {
        try
        {
            using var client = SqsConfigBuilder.BuildClient(request.Region, credential);

            var createRequest = new CreateQueueRequest
            {
                QueueName = request.QueueName,
                Attributes = new Dictionary<string, string>
                {
                    { "VisibilityTimeout", request.VisibilityTimeout.ToString() },
                    { "MessageRetentionPeriod", request.MessageRetentionPeriod.ToString() },
                    { "MaximumMessageSize", request.MaximumMessageSize.ToString() }
                }
            };

            if (request.IsFifo)
            {
                createRequest.Attributes.Add("FifoQueue", "true");
                createRequest.Attributes.Add("ContentBasedDeduplication", "true");
            }

            if (!string.IsNullOrEmpty(request.DeadLetterQueueArn))
            {
                var redrivePolicy = JsonSerializer.Serialize(new
                {
                    deadLetterTargetArn = request.DeadLetterQueueArn,
                    maxReceiveCount = request.MaxReceiveCount
                });
                createRequest.Attributes.Add("RedrivePolicy", redrivePolicy);
            }

            var response = await client.CreateQueueAsync(createRequest);

            // Audit logging
            var details = JsonSerializer.Serialize(new
            {
                queueName = request.QueueName,
                region = request.Region,
                queueUrl = response.QueueUrl,
                isFifo = request.IsFifo
            });

            await auditLog.Audit(
                ActivityTypeEnum.AddSQSQueue,
                $"SQS queue '{request.QueueName}' created in region '{request.Region}'",
                details);

            logger.LogInformation("Created queue {QueueName} in region {Region}", request.QueueName, request.Region);
            return response.QueueUrl;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating queue {QueueName}", request.QueueName);
            throw;
        }
    }

    public async Task<bool> DeleteQueueAsync(string region, string queueUrl, ServerCredential credential)
    {
        try
        {
            using var client = SqsConfigBuilder.BuildClient(region, credential);

            var queueName = ExtractQueueName(queueUrl);

            // Audit log BEFORE deletion
            var details = JsonSerializer.Serialize(new
            {
                queueName,
                region,
                queueUrl
            });

            await auditLog.Audit(
                ActivityTypeEnum.RemoveSQSQueue,
                $"SQS queue '{queueName}' deleted from region '{region}'",
                details);

            await client.DeleteQueueAsync(queueUrl);

            logger.LogInformation("Deleted queue {QueueUrl}", queueUrl);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting queue {QueueUrl}", queueUrl);
            throw;
        }
    }

    public async Task<bool> SendMessageAsync(SqsSendMessageRequest request, ServerCredential credential)
    {
        try
        {
            using var client = SqsConfigBuilder.BuildClient(request.Region, credential);

            var sendRequest = new Amazon.SQS.Model.SendMessageRequest
            {
                QueueUrl = request.QueueUrl,
                MessageBody = request.MessageBody,
                DelaySeconds = request.DelaySeconds
            };

            if (request.MessageAttributes != null)
            {
                foreach (var attr in request.MessageAttributes)
                {
                    sendRequest.MessageAttributes.Add(attr.Key, new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = attr.Value
                    });
                }
            }

            if (!string.IsNullOrEmpty(request.MessageGroupId))
            {
                sendRequest.MessageGroupId = request.MessageGroupId;
            }

            if (!string.IsNullOrEmpty(request.MessageDeduplicationId))
            {
                sendRequest.MessageDeduplicationId = request.MessageDeduplicationId;
            }

            var response = await client.SendMessageAsync(sendRequest);

            // Audit logging
            var details = JsonSerializer.Serialize(new
            {
                queueUrl = request.QueueUrl,
                messageId = response.MessageId,
                bodyLength = request.MessageBody.Length
            });

            await auditLog.Audit(
                ActivityTypeEnum.SendSQSMessage,
                $"Message sent to SQS queue '{ExtractQueueName(request.QueueUrl)}'",
                details);

            logger.LogInformation("Sent message to queue {QueueUrl}", request.QueueUrl);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending message to queue {QueueUrl}", request.QueueUrl);
            throw;
        }
    }

    public async Task<IEnumerable<SqsMessage>> ReceiveMessagesAsync(SqsReceiveMessagesRequest request, ServerCredential credential)
    {
        try
        {
            using var client = SqsConfigBuilder.BuildClient(request.Region, credential);

            var receiveRequest = new ReceiveMessageRequest
            {
                QueueUrl = request.QueueUrl,
                MaxNumberOfMessages = Math.Min(request.MaxMessages, 10),  // AWS limit: 10
                VisibilityTimeout = request.VisibilityTimeout,
                WaitTimeSeconds = request.WaitTimeSeconds,
                AttributeNames = new List<string> { "All" },
                MessageAttributeNames = new List<string> { "All" }
            };

            var response = await client.ReceiveMessageAsync(receiveRequest);

            var messages = response.Messages.Select(m => new SqsMessage(
                m.MessageId,
                m.Body,
                m.ReceiptHandle,
                m.Attributes,
                m.MessageAttributes
            )).ToList();

            logger.LogInformation("Received {Count} messages from queue {QueueUrl}", messages.Count, request.QueueUrl);
            return messages;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error receiving messages from queue {QueueUrl}", request.QueueUrl);
            throw;
        }
    }

    public async Task<bool> DeleteMessageAsync(string region, string queueUrl, string receiptHandle, ServerCredential credential)
    {
        try
        {
            using var client = SqsConfigBuilder.BuildClient(region, credential);

            await client.DeleteMessageAsync(queueUrl, receiptHandle);

            logger.LogInformation("Deleted message from queue {QueueUrl}", queueUrl);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting message from queue {QueueUrl}", queueUrl);
            throw;
        }
    }

    public async Task<bool> TestConnectionAsync(string region, ServerCredential credential)
    {
        try
        {
            using var client = SqsConfigBuilder.BuildClient(region, credential);
            await client.ListQueuesAsync(new ListQueuesRequest());
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Connection test failed for region {Region}", region);
            return false;
        }
    }

    /// <summary>
    /// Extracts queue name from queue URL
    /// </summary>
    private string ExtractQueueName(string queueUrl)
    {
        return queueUrl.Split('/').Last();
    }
}

namespace Nanuq.AWS.SQS.Entities;

/// <summary>
/// Represents detailed information about an SQS queue
/// </summary>
/// <param name="QueueName">Name of the queue</param>
/// <param name="QueueUrl">Full URL of the queue</param>
/// <param name="ApproximateMessages">Approximate number of messages available</param>
/// <param name="ApproximateMessagesNotVisible">Approximate number of messages in flight</param>
/// <param name="ApproximateMessagesDelayed">Approximate number of delayed messages</param>
/// <param name="CreatedTimestamp">When the queue was created</param>
/// <param name="LastModifiedTimestamp">When the queue was last modified</param>
/// <param name="VisibilityTimeout">Message visibility timeout in seconds</param>
/// <param name="MessageRetentionPeriod">Message retention period in seconds</param>
/// <param name="MaximumMessageSize">Maximum message size in bytes</param>
/// <param name="DeadLetterQueueUrl">Dead letter queue URL if configured</param>
/// <param name="IsFifo">Whether this is a FIFO queue</param>
public record SqsQueueDetails(
    string QueueName,
    string QueueUrl,
    int ApproximateMessages,
    int ApproximateMessagesNotVisible,
    int ApproximateMessagesDelayed,
    DateTime CreatedTimestamp,
    DateTime LastModifiedTimestamp,
    int VisibilityTimeout,
    int MessageRetentionPeriod,
    int MaximumMessageSize,
    string? DeadLetterQueueUrl,
    bool IsFifo);

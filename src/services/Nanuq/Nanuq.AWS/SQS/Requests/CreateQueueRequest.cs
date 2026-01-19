namespace Nanuq.AWS.SQS.Requests;

/// <summary>
/// Request to create a new SQS queue
/// </summary>
/// <param name="Region">AWS region where queue will be created</param>
/// <param name="QueueName">Name of the queue to create</param>
/// <param name="VisibilityTimeout">Message visibility timeout in seconds (default: 30)</param>
/// <param name="MessageRetentionPeriod">Message retention period in seconds (default: 4 days)</param>
/// <param name="MaximumMessageSize">Maximum message size in bytes (default: 256 KB)</param>
/// <param name="DeadLetterQueueArn">ARN of dead letter queue (optional)</param>
/// <param name="MaxReceiveCount">Max receive count before sending to DLQ (default: 5)</param>
/// <param name="IsFifo">Whether this is a FIFO queue (default: false)</param>
public record CreateQueueRequest(
    string Region,
    string QueueName,
    int VisibilityTimeout = 30,
    int MessageRetentionPeriod = 345600,  // 4 days
    int MaximumMessageSize = 262144,      // 256 KB
    string? DeadLetterQueueArn = null,
    int MaxReceiveCount = 5,
    bool IsFifo = false);

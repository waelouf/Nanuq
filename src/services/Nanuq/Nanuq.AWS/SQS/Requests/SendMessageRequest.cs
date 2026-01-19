namespace Nanuq.AWS.SQS.Requests;

/// <summary>
/// Request to send a message to an SQS queue
/// </summary>
/// <param name="Region">AWS region</param>
/// <param name="QueueUrl">Queue URL</param>
/// <param name="MessageBody">Message body content</param>
/// <param name="MessageAttributes">Custom message attributes (optional)</param>
/// <param name="DelaySeconds">Delay before message becomes visible (default: 0)</param>
/// <param name="MessageGroupId">Message group ID (required for FIFO queues)</param>
/// <param name="MessageDeduplicationId">Message deduplication ID (required for FIFO queues without content-based deduplication)</param>
public record SendMessageRequest(
    string Region,
    string QueueUrl,
    string MessageBody,
    Dictionary<string, string>? MessageAttributes = null,
    int DelaySeconds = 0,
    string? MessageGroupId = null,
    string? MessageDeduplicationId = null);

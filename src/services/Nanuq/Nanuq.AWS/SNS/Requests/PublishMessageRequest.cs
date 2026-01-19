namespace Nanuq.AWS.SNS.Requests;

/// <summary>
/// Request to publish a message to an SNS topic
/// </summary>
/// <param name="Region">AWS region</param>
/// <param name="TopicArn">Topic ARN</param>
/// <param name="Message">Message content</param>
/// <param name="Subject">Message subject (optional, used for email subscriptions)</param>
/// <param name="MessageAttributes">Custom message attributes (optional)</param>
/// <param name="MessageGroupId">Message group ID (required for FIFO topics)</param>
/// <param name="MessageDeduplicationId">Message deduplication ID (required for FIFO topics without content-based deduplication)</param>
public record PublishMessageRequest(
    string Region,
    string TopicArn,
    string Message,
    string? Subject = null,
    Dictionary<string, string>? MessageAttributes = null,
    string? MessageGroupId = null,
    string? MessageDeduplicationId = null);

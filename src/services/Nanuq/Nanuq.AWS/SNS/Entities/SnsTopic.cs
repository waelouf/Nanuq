namespace Nanuq.AWS.SNS.Entities;

/// <summary>
/// Represents an SNS topic with basic information
/// </summary>
/// <param name="TopicArn">Amazon Resource Name (ARN) of the topic</param>
/// <param name="TopicName">Name of the topic</param>
/// <param name="SubscriptionsCount">Number of subscriptions to the topic</param>
public record SnsTopic(
    string TopicArn,
    string TopicName,
    int SubscriptionsCount);

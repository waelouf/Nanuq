namespace Nanuq.AWS.SNS.Entities;

/// <summary>
/// Represents detailed information about an SNS topic
/// </summary>
/// <param name="TopicArn">Amazon Resource Name (ARN) of the topic</param>
/// <param name="TopicName">Name of the topic</param>
/// <param name="DisplayName">Display name for the topic</param>
/// <param name="Owner">AWS account ID of the topic owner</param>
/// <param name="SubscriptionsConfirmed">Number of confirmed subscriptions</param>
/// <param name="SubscriptionsPending">Number of pending subscriptions</param>
/// <param name="SubscriptionsDeleted">Number of deleted subscriptions</param>
/// <param name="CreatedAt">When the topic was created</param>
public record SnsTopicDetails(
    string TopicArn,
    string TopicName,
    string DisplayName,
    string Owner,
    int SubscriptionsConfirmed,
    int SubscriptionsPending,
    int SubscriptionsDeleted,
    DateTime CreatedAt);

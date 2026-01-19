using Nanuq.AWS.SNS.Entities;
using Nanuq.Common.Records;
using SnsCreateTopicRequest = Nanuq.AWS.SNS.Requests.CreateTopicRequest;
using SnsPublishMessageRequest = Nanuq.AWS.SNS.Requests.PublishMessageRequest;
using SnsSubscribeRequest = Nanuq.AWS.SNS.Requests.SubscribeRequest;

namespace Nanuq.AWS.SNS.Interfaces;

/// <summary>
/// Interface for SNS topic and subscription management operations
/// </summary>
public interface ISnsManagerRepository
{
    /// <summary>
    /// Gets all topics in the specified region
    /// </summary>
    Task<IEnumerable<SnsTopic>> GetTopicsAsync(string region, ServerCredential credential);

    /// <summary>
    /// Gets detailed information about a specific topic
    /// </summary>
    Task<SnsTopicDetails> GetTopicDetailsAsync(string region, string topicArn, ServerCredential credential);

    /// <summary>
    /// Creates a new SNS topic
    /// </summary>
    Task<string> CreateTopicAsync(SnsCreateTopicRequest request, ServerCredential credential);

    /// <summary>
    /// Deletes an SNS topic
    /// </summary>
    Task<bool> DeleteTopicAsync(string region, string topicArn, ServerCredential credential);

    /// <summary>
    /// Publishes a message to an SNS topic
    /// </summary>
    Task<string> PublishMessageAsync(SnsPublishMessageRequest request, ServerCredential credential);

    /// <summary>
    /// Gets subscriptions for a topic
    /// </summary>
    Task<IEnumerable<SnsSubscription>> GetSubscriptionsAsync(string region, string topicArn, ServerCredential credential);

    /// <summary>
    /// Subscribes an endpoint to a topic
    /// </summary>
    Task<string> SubscribeAsync(SnsSubscribeRequest request, ServerCredential credential);

    /// <summary>
    /// Unsubscribes an endpoint from a topic
    /// </summary>
    Task<bool> UnsubscribeAsync(string region, string subscriptionArn, ServerCredential credential);

    /// <summary>
    /// Tests connection to AWS SNS
    /// </summary>
    Task<bool> TestConnectionAsync(string region, ServerCredential credential);
}

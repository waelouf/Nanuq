using Nanuq.AWS.SQS.Entities;
using Nanuq.Common.Records;
using SqsCreateQueueRequest = Nanuq.AWS.SQS.Requests.CreateQueueRequest;
using SqsSendMessageRequest = Nanuq.AWS.SQS.Requests.SendMessageRequest;
using SqsReceiveMessagesRequest = Nanuq.AWS.SQS.Requests.ReceiveMessagesRequest;

namespace Nanuq.AWS.SQS.Interfaces;

/// <summary>
/// Interface for SQS queue and message management operations
/// </summary>
public interface ISqsManagerRepository
{
    /// <summary>
    /// Gets all queues in the specified region
    /// </summary>
    Task<IEnumerable<SqsQueue>> GetQueuesAsync(string region, ServerCredential credential);

    /// <summary>
    /// Gets detailed information about a specific queue
    /// </summary>
    Task<SqsQueueDetails> GetQueueDetailsAsync(string region, string queueUrl, ServerCredential credential);

    /// <summary>
    /// Creates a new SQS queue
    /// </summary>
    Task<string> CreateQueueAsync(SqsCreateQueueRequest request, ServerCredential credential);

    /// <summary>
    /// Deletes an SQS queue
    /// </summary>
    Task<bool> DeleteQueueAsync(string region, string queueUrl, ServerCredential credential);

    /// <summary>
    /// Sends a message to an SQS queue
    /// </summary>
    Task<bool> SendMessageAsync(SqsSendMessageRequest request, ServerCredential credential);

    /// <summary>
    /// Receives messages from an SQS queue
    /// </summary>
    Task<IEnumerable<SqsMessage>> ReceiveMessagesAsync(SqsReceiveMessagesRequest request, ServerCredential credential);

    /// <summary>
    /// Deletes a message from an SQS queue
    /// </summary>
    Task<bool> DeleteMessageAsync(string region, string queueUrl, string receiptHandle, ServerCredential credential);

    /// <summary>
    /// Tests connection to AWS SQS
    /// </summary>
    Task<bool> TestConnectionAsync(string region, ServerCredential credential);
}

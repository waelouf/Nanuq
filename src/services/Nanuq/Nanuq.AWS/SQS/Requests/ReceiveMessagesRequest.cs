namespace Nanuq.AWS.SQS.Requests;

/// <summary>
/// Request to receive messages from an SQS queue
/// </summary>
/// <param name="Region">AWS region</param>
/// <param name="QueueUrl">Queue URL</param>
/// <param name="MaxMessages">Maximum number of messages to receive (default: 10, AWS max: 10)</param>
/// <param name="VisibilityTimeout">Visibility timeout in seconds (default: 30)</param>
/// <param name="WaitTimeSeconds">Long polling wait time (0 = short polling, 1-20 = long polling, default: 0)</param>
public record ReceiveMessagesRequest(
    string Region,
    string QueueUrl,
    int MaxMessages = 10,
    int VisibilityTimeout = 30,
    int WaitTimeSeconds = 0);

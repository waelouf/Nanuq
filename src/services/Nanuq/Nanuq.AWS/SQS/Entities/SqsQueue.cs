namespace Nanuq.AWS.SQS.Entities;

/// <summary>
/// Represents an SQS queue with basic information
/// </summary>
/// <param name="QueueName">Name of the queue</param>
/// <param name="QueueUrl">Full URL of the queue</param>
/// <param name="ApproximateMessages">Approximate number of messages in the queue</param>
public record SqsQueue(
    string QueueName,
    string QueueUrl,
    int ApproximateMessages);

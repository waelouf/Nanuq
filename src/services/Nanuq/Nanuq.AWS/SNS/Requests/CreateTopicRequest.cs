namespace Nanuq.AWS.SNS.Requests;

/// <summary>
/// Request to create a new SNS topic
/// </summary>
/// <param name="Region">AWS region where topic will be created</param>
/// <param name="TopicName">Name of the topic to create</param>
/// <param name="DisplayName">Display name for the topic (optional)</param>
/// <param name="IsFifo">Whether this is a FIFO topic (default: false)</param>
public record CreateTopicRequest(
    string Region,
    string TopicName,
    string? DisplayName = null,
    bool IsFifo = false);

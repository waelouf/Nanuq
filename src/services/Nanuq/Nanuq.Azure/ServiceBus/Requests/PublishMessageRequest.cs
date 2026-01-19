namespace Nanuq.Azure.ServiceBus.Requests;

public class PublishMessageRequest
{
    public int ServerId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public Dictionary<string, object>? ApplicationProperties { get; set; }
}

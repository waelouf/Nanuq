namespace Nanuq.Azure.ServiceBus.Requests;

public class SendMessageRequest
{
    public int ServerId { get; set; }
    public string QueueName { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public Dictionary<string, object>? ApplicationProperties { get; set; }
}

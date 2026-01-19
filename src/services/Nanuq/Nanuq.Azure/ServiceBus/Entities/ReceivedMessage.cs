namespace Nanuq.Azure.ServiceBus.Entities;

public class ReceivedMessage
{
    public string MessageId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTimeOffset EnqueuedTime { get; set; }
    public int DeliveryCount { get; set; }
    public Dictionary<string, object> ApplicationProperties { get; set; } = new();
}

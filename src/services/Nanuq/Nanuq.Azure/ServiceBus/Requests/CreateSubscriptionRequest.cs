namespace Nanuq.Azure.ServiceBus.Requests;

public class CreateSubscriptionRequest
{
    public int ServerId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public string SubscriptionName { get; set; } = string.Empty;
    public int MaxDeliveryCount { get; set; } = 10;
    public TimeSpan? LockDuration { get; set; }
    public bool RequiresSession { get; set; } = false;
    public bool DeadLetteringOnMessageExpiration { get; set; } = false;
}

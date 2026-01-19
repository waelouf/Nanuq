namespace Nanuq.Azure.ServiceBus.Entities;

public class ServiceBusSubscription
{
    public string TopicName { get; set; } = string.Empty;
    public string SubscriptionName { get; set; } = string.Empty;
    public long MessageCount { get; set; }
    public long DeadLetterMessageCount { get; set; }
    public TimeSpan LockDuration { get; set; }
    public bool RequiresSession { get; set; }
    public TimeSpan DefaultMessageTimeToLive { get; set; }
    public TimeSpan AutoDeleteOnIdle { get; set; }
    public bool DeadLetteringOnMessageExpiration { get; set; }
    public int MaxDeliveryCount { get; set; }
    public string Status { get; set; } = "Active";
}

namespace Nanuq.Azure.ServiceBus.Entities;

public class ServiceBusQueue
{
    public string Name { get; set; } = string.Empty;
    public long MessageCount { get; set; }
    public long DeadLetterMessageCount { get; set; }
    public long ScheduledMessageCount { get; set; }
    public long TransferMessageCount { get; set; }
    public long TransferDeadLetterMessageCount { get; set; }
    public TimeSpan LockDuration { get; set; }
    public long MaxSizeInMegabytes { get; set; }
    public bool RequiresDuplicateDetection { get; set; }
    public bool RequiresSession { get; set; }
    public TimeSpan DefaultMessageTimeToLive { get; set; }
    public TimeSpan AutoDeleteOnIdle { get; set; }
    public bool DeadLetteringOnMessageExpiration { get; set; }
    public int MaxDeliveryCount { get; set; }
    public bool EnableBatchedOperations { get; set; }
    public string Status { get; set; } = "Active";
}

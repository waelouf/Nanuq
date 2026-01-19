namespace Nanuq.Azure.ServiceBus.Requests;

public class CreateQueueRequest
{
    public int ServerId { get; set; }
    public string QueueName { get; set; } = string.Empty;
    public int MaxSizeInMegabytes { get; set; } = 1024;
    public TimeSpan? DefaultMessageTimeToLive { get; set; }
    public TimeSpan? LockDuration { get; set; }
    public int MaxDeliveryCount { get; set; } = 10;
    public bool RequiresDuplicateDetection { get; set; } = false;
    public bool RequiresSession { get; set; } = false;
    public bool DeadLetteringOnMessageExpiration { get; set; } = false;
}

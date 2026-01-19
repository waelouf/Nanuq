namespace Nanuq.Azure.ServiceBus.Entities;

public class ServiceBusTopic
{
    public string Name { get; set; } = string.Empty;
    public long SubscriptionCount { get; set; }
    public long MaxSizeInMegabytes { get; set; }
    public bool RequiresDuplicateDetection { get; set; }
    public TimeSpan DefaultMessageTimeToLive { get; set; }
    public TimeSpan AutoDeleteOnIdle { get; set; }
    public bool EnableBatchedOperations { get; set; }
    public bool SupportOrdering { get; set; }
    public string Status { get; set; } = "Active";
}

namespace Nanuq.Azure.ServiceBus.Requests;

public class CreateTopicRequest
{
    public int ServerId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public int MaxSizeInMegabytes { get; set; } = 1024;
    public TimeSpan? DefaultMessageTimeToLive { get; set; }
    public bool RequiresDuplicateDetection { get; set; } = false;
    public bool SupportOrdering { get; set; } = false;
}

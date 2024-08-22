namespace Nanuq.Kafka.Requests;

public class AddKafkaTopicRequest
{
    public string BootstrapServers { get; set; }

    public string TopicName { get; set; }

    public int NumberOfPartitions { get; set; } = 1;

    public short ReplicationFactor { get; set; } = 1;

}

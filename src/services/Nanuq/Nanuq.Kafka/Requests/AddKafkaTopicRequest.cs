namespace Nanuq.Kafka.Requests;

public record AddKafkaTopicRequest(string BootstrapServers, string TopicName)
{
	public int NumberOfPartitions { get; set; } = 1;

	public short ReplicationFactor { get; set; } = 1;

}

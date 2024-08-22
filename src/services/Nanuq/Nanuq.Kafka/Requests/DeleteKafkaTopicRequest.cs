namespace Nanuq.Kafka.Requests;

public class DeleteKafkaTopicRequest
{
    public string BootstrapServers { get; set; }

    public string TopicName { get; set; }
}

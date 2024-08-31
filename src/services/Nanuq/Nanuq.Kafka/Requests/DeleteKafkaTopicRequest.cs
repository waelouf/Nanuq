namespace Nanuq.Kafka.Requests;

public record DeleteKafkaTopicRequest(string BootstrapServer, string TopicName);

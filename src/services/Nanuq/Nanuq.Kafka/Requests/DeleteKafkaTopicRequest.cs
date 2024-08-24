namespace Nanuq.Kafka.Requests;

public record DeleteKafkaTopicRequest(string BootstrapServers, string TopicName);

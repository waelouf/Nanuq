using Nanuq.Kafka.Entities;
using Nanuq.Kafka.Requests;

namespace Nanuq.Kafka.Interfaces;

public interface ITopicsRepository
{
	Task<IEnumerable<Topic>> GetTopicsAsync(string bootstrapServers);

	Task<TopicDetails> GetTopicDetailsAsync(string bootstrapServers, string topicName);

	Task<bool> DeleteTopicAsync(DeleteKafkaTopicRequest topicRequest);

	Task<bool> AddTopicAsync(AddKafkaTopicRequest topicRequest);
}

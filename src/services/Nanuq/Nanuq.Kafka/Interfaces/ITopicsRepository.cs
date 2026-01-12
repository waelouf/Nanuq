using Nanuq.Common.Records;
using Nanuq.Kafka.Entities;
using Nanuq.Kafka.Requests;

namespace Nanuq.Kafka.Interfaces;

public interface ITopicsRepository
{
	Task<IEnumerable<Topic>> GetTopicsAsync(string bootstrapServers, ServerCredential? credential = null);

	Task<TopicDetails> GetTopicDetailsAsync(string bootstrapServers, string topicName, ServerCredential? credential = null);

	Task<bool> DeleteTopicAsync(DeleteKafkaTopicRequest topicRequest, ServerCredential? credential = null);

	Task<bool> AddTopicAsync(AddKafkaTopicRequest topicRequest, ServerCredential? credential = null);
}

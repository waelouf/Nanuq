using Confluent.Kafka;

namespace Nanuq.Kafka;

public class Topics
{
    private string bootstrapServers;

	public Topics(string bootstrapServers)
	{
		this.bootstrapServers = bootstrapServers;
	}

	public IEnumerable<TopicMetadata> GetTopics()
	{
		var config = new AdminClientConfig
		{
			BootstrapServers = bootstrapServers
		};

		using var adminClient = new AdminClientBuilder(config).Build();
		try
		{
			var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
			return metadata.Topics;
		}
		catch (KafkaException kExc)
		{
			return null;
		}
	}
}

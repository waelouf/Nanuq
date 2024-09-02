using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Kafka.Entities;
using Nanuq.Kafka.Interfaces;
using Nanuq.Kafka.Requests;

namespace Nanuq.Kafka.Repositories;

public class TopicsRepository : ITopicsRepository
{
	private ILogger<TopicsRepository> logger;

	private IAuditLogRepository activityLog;

	public TopicsRepository(ILogger<TopicsRepository> logger, IAuditLogRepository activityLog)
	{
		this.logger = logger;
		this.activityLog = activityLog;
	}

	public async Task<IEnumerable<Topic>> GetTopicsAsync(string bootstrapServers)
    {
		var config = new AdminClientConfig
        {
            BootstrapServers = bootstrapServers
        };

        using var adminClient = new AdminClientBuilder(config).Build();
        try
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10))
				.Topics.Select(t => new Topic(t.Topic, t.Partitions.Count));
			return await Task.FromResult(metadata);
        }
        catch (KafkaException kExc)
        {
			logger.LogError(kExc.Message, kExc);
            return null;
        }
    }

	public async Task<TopicDetails> GetTopicDetailsAsync(string bootstrapServers, string topicName)
	{
		long messagesCount = 0;

		var config = new AdminClientConfig
		{
			BootstrapServers = bootstrapServers
		};


		using var adminClient = new AdminClientBuilder(config).Build();

		using var consumer = new ConsumerBuilder<Ignore, Ignore>(new ConsumerConfig
		{
			BootstrapServers = bootstrapServers,
			GroupId = new Guid().ToString(),
			EnableAutoCommit = false
		}).Build();

		try
		{
			// Get the topic metadata
			var metadata = adminClient.GetMetadata(topicName, TimeSpan.FromSeconds(10));
			var topicMetadata = metadata.Topics[0];

			foreach (var partition in topicMetadata.Partitions)
			{
				var partitionOffset = consumer.QueryWatermarkOffsets(new TopicPartition(topicName, partition.PartitionId), TimeSpan.FromSeconds(10));
				long partitionMessageCount = partitionOffset.High.Value - partitionOffset.Low.Value;

				messagesCount += partitionMessageCount;
			}

		}
		catch (KafkaException e)
		{
			logger.LogError(e.Message, e);
			return null;
		}

		var topicDetails = new TopicDetails(topicName, messagesCount);
		return await Task.FromResult(topicDetails);
	}

	public async Task<bool> DeleteTopicAsync(DeleteKafkaTopicRequest request)
	{
		bool deleted = false;
		var config = new AdminClientConfig
		{
			BootstrapServers = request.BootstrapServer
		};

		using var adminClient = new AdminClientBuilder(config).Build();
		try
		{
			// Deleting the topic
			await adminClient.DeleteTopicsAsync(new List<string> { request.TopicName });
			deleted = true;
		}
		catch (DeleteTopicsException e)
		{
			logger.LogError(e.Message, e);
			throw;
		}
		catch (KafkaException e)
		{
			logger.LogError(e.Message, e);
			throw;
		}

		if(deleted)
		{
			activityLog.Audit(ActivityTypeEnum.RemoveKafkaTopic,
				$"Topic {request.TopicName} removed successfully to the server ${request.BootstrapServer}");
		}

		return await Task.FromResult(deleted);
	}

	public async Task<bool> AddTopicAsync(AddKafkaTopicRequest topicRequest)
	{
		var created = false;

		var config = new AdminClientConfig
		{
			BootstrapServers = topicRequest.BootstrapServers
		};

		using var adminClient = new AdminClientBuilder(config).Build();

		try
		{
			var topicSpecification = new TopicSpecification
			{
				Name = topicRequest.TopicName,
				NumPartitions = topicRequest.NumberOfPartitions,
				ReplicationFactor = topicRequest.ReplicationFactor
			};

			await adminClient.CreateTopicsAsync(new List<TopicSpecification> { topicSpecification });
			created = true;
		}
		catch (CreateTopicsException e)
		{
			logger.LogError(e.Message, e);
			throw;
		}
		catch (KafkaException e)
		{
			logger.LogError(e.Message, e);
			throw;
		}

		if (created)
		{
			activityLog.Audit(ActivityTypeEnum.AddKafkaTopic,
				$"Topic {topicRequest.TopicName} removed successfully to the server ${topicRequest.BootstrapServers}");
		}

		return await Task.FromResult(created);
	}
}

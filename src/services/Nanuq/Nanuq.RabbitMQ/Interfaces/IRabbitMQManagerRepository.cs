using Nanuq.Common.Records;
using Nanuq.RabbitMQ.Entities;
using Nanuq.RabbitMQ.Requests;

namespace Nanuq.RabbitMQ.Interfaces;

public interface IRabbitMQManagerRepository
{
	// Exchange operations
	Task<IEnumerable<Exchange>> GetExchangesAsync(string serverUrl, ServerCredential? credential = null);
	Task<bool> AddExchangeAsync(AddExchangeRequest request, ServerCredential? credential = null);
	Task<bool> DeleteExchangeAsync(DeleteExchangeRequest request, ServerCredential? credential = null);

	// Queue operations
	Task<IEnumerable<Queue>> GetQueuesAsync(string serverUrl, ServerCredential? credential = null);
	Task<QueueDetails> GetQueueDetailsAsync(string serverUrl, string queueName, ServerCredential? credential = null);
	Task<bool> AddQueueAsync(AddQueueRequest request, ServerCredential? credential = null);
	Task<bool> DeleteQueueAsync(DeleteQueueRequest request, ServerCredential? credential = null);

	// Connection test
	Task<bool> TestConnectionAsync(string serverUrl, ServerCredential credential);
}

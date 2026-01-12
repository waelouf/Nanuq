using Nanuq.Common.Records;

namespace Nanuq.RabbitMQ.Interfaces;

public interface IRabbitMQManagerRepository
{
	void GetConnection(string serverUrl, ServerCredential? credential = null);
}

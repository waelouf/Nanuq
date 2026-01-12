using Nanuq.Common.Records;
using Nanuq.RabbitMQ.Helpers;
using Nanuq.RabbitMQ.Interfaces;
using RabbitMQ.Client;

namespace Nanuq.RabbitMQ.Repository;

public class RabbitMQManagerRepository : IRabbitMQManagerRepository
{
	public void GetConnection(string serverUrl, ServerCredential? credential = null)
	{
		var factory = RabbitMQConfigBuilder.BuildConnectionFactory(serverUrl, credential);
		using var connection = factory.CreateConnectionAsync();

		// Not implemented yet
	}
}

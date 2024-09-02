using Nanuq.RabbitMQ.Interfaces;
using RabbitMQ.Client;

namespace Nanuq.RabbitMQ.Repository;

public class RabbitMQManagerRepository : IRabbitMQManagerRepository
{
	public void GetConnection(string serverUrl, string username, string password)
	{
		var urlParts = serverUrl.Split(':');

		var factory = new ConnectionFactory() { HostName = urlParts[0], Port = int.Parse(urlParts[1]), UserName = username, Password = password };
		using var connection = factory.CreateConnection();

		// Not implemented yet
	}
}

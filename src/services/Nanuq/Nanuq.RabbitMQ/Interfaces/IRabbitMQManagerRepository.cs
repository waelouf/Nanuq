namespace Nanuq.RabbitMQ.Interfaces;

public interface IRabbitMQManagerRepository
{
	void GetConnection(string serverUrl, string username, string password);
}

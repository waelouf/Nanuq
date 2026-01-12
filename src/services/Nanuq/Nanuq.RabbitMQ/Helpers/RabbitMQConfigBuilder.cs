using Nanuq.Common.Records;
using RabbitMQ.Client;

namespace Nanuq.RabbitMQ.Helpers;

public static class RabbitMQConfigBuilder
{
	public static ConnectionFactory BuildConnectionFactory(
		string serverUrl,
		ServerCredential? credential = null)
	{
		var urlParts = serverUrl.Split(':');
		var hostName = urlParts[0];
		var port = urlParts.Length > 1 ? int.Parse(urlParts[1]) : 5672; // Default RabbitMQ port

		var factory = new ConnectionFactory
		{
			HostName = hostName,
			Port = port
		};

		if (credential != null)
		{
			if (!string.IsNullOrEmpty(credential.Username))
			{
				factory.UserName = credential.Username;
			}

			if (!string.IsNullOrEmpty(credential.Password))
			{
				factory.Password = credential.Password;
			}
		}
		else
		{
			// Default RabbitMQ guest credentials (only works on localhost)
			factory.UserName = "guest";
			factory.Password = "guest";
		}

		return factory;
	}
}

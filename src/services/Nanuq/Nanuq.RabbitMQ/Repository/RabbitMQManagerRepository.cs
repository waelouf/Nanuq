using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.RabbitMQ.Entities;
using Nanuq.RabbitMQ.Helpers;
using Nanuq.RabbitMQ.Interfaces;
using Nanuq.RabbitMQ.Requests;
using RabbitMQ.Client;

namespace Nanuq.RabbitMQ.Repository;

public class RabbitMQManagerRepository : IRabbitMQManagerRepository
{
	private readonly IAuditLogRepository _auditLog;

	public RabbitMQManagerRepository(IAuditLogRepository auditLog)
	{
		_auditLog = auditLog;
	}

	// Internal DTOs for Management API responses
	private class ManagementExchange
	{
		public string name { get; set; } = string.Empty;
		public string type { get; set; } = string.Empty;
		public bool durable { get; set; }
		public bool auto_delete { get; set; }
		public Dictionary<string, object>? arguments { get; set; }
	}

	private class ManagementQueue
	{
		public string name { get; set; } = string.Empty;
		public int messages { get; set; }
		public int consumers { get; set; }
		public bool durable { get; set; }
		public bool auto_delete { get; set; }
		public bool exclusive { get; set; }
		public long memory { get; set; }
		public Dictionary<string, object>? arguments { get; set; }
	}

	private async Task<string> GetManagementBaseUrl(string serverUrl)
	{
		var urlParts = serverUrl.Split(':');
		var host = urlParts[0];
		// Management API runs on port 15672 by default
		return $"http://{host}:15672/api";
	}

	private HttpClient CreateManagementClient(ServerCredential? credential)
	{
		var client = new HttpClient();

		if (credential != null)
		{
			var authBytes = Encoding.ASCII.GetBytes($"{credential.Username}:{credential.Password}");
			client.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
		}
		else
		{
			// Default to guest:guest
			var authBytes = Encoding.ASCII.GetBytes("guest:guest");
			client.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
		}

		return client;
	}

	// Exchange operations
	public async Task<IEnumerable<Exchange>> GetExchangesAsync(string serverUrl, ServerCredential? credential = null)
	{
		try
		{
			var baseUrl = await GetManagementBaseUrl(serverUrl);
			using var client = CreateManagementClient(credential);

			// Get exchanges from default vhost
			var response = await client.GetAsync($"{baseUrl}/exchanges/%2F");
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();
			var managementExchanges = JsonSerializer.Deserialize<List<ManagementExchange>>(json) ?? new List<ManagementExchange>();

			// Map to domain entities
			return managementExchanges.Select(e => new Exchange(
				e.name,
				e.type,
				e.durable,
				e.auto_delete,
				e.arguments
			));
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to get exchanges from RabbitMQ Management API: {ex.Message}", ex);
		}
	}

	public async Task<bool> AddExchangeAsync(AddExchangeRequest request, ServerCredential? credential = null)
	{
		try
		{
			var factory = RabbitMQConfigBuilder.BuildConnectionFactory(request.ServerUrl, credential);
			await using var connection = await factory.CreateConnectionAsync();
			await using var channel = await connection.CreateChannelAsync();

			await channel.ExchangeDeclareAsync(
				exchange: request.Name,
				type: request.Type,
				durable: request.Durable,
				autoDelete: request.AutoDelete);

			// Audit log after successful creation
			var details = JsonSerializer.Serialize(new
			{
				exchangeName = request.Name,
				exchangeType = request.Type,
				durable = request.Durable,
				autoDelete = request.AutoDelete,
				serverUrl = request.ServerUrl
			});
			await _auditLog.Audit(
				ActivityTypeEnum.AddRabbitMQExchange,
				$"RabbitMQ exchange '{request.Name}' created on server '{request.ServerUrl}'",
				details);

			return true;
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to create exchange: {ex.Message}", ex);
		}
	}

	public async Task<bool> DeleteExchangeAsync(DeleteExchangeRequest request, ServerCredential? credential = null)
	{
		try
		{
			// Audit log before deletion
			var details = JsonSerializer.Serialize(new
			{
				exchangeName = request.Name,
				serverUrl = request.ServerUrl
			});
			await _auditLog.Audit(
				ActivityTypeEnum.RemoveRabbitMQExchange,
				$"RabbitMQ exchange '{request.Name}' deleted from server '{request.ServerUrl}'",
				details);

			var factory = RabbitMQConfigBuilder.BuildConnectionFactory(request.ServerUrl, credential);
			await using var connection = await factory.CreateConnectionAsync();
			await using var channel = await connection.CreateChannelAsync();

			await channel.ExchangeDeleteAsync(exchange: request.Name);

			return true;
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to delete exchange: {ex.Message}", ex);
		}
	}

	// Queue operations
	public async Task<IEnumerable<Queue>> GetQueuesAsync(string serverUrl, ServerCredential? credential = null)
	{
		try
		{
			var baseUrl = await GetManagementBaseUrl(serverUrl);
			using var client = CreateManagementClient(credential);

			// Get queues from default vhost
			var response = await client.GetAsync($"{baseUrl}/queues/%2F");
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();
			var managementQueues = JsonSerializer.Deserialize<List<ManagementQueue>>(json) ?? new List<ManagementQueue>();

			// Map to domain entities
			return managementQueues.Select(q => new Queue(
				q.name,
				q.messages,
				q.consumers,
				q.durable,
				q.auto_delete,
				q.exclusive
			));
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to get queues from RabbitMQ Management API: {ex.Message}", ex);
		}
	}

	public async Task<QueueDetails> GetQueueDetailsAsync(string serverUrl, string queueName, ServerCredential? credential = null)
	{
		try
		{
			var baseUrl = await GetManagementBaseUrl(serverUrl);
			using var client = CreateManagementClient(credential);

			// Get specific queue details from default vhost
			var encodedQueueName = Uri.EscapeDataString(queueName);
			var response = await client.GetAsync($"{baseUrl}/queues/%2F/{encodedQueueName}");
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();
			var queue = JsonSerializer.Deserialize<ManagementQueue>(json) ?? throw new Exception("Failed to deserialize queue details");

			// Map to domain entity
			return new QueueDetails(
				queue.name,
				queue.messages,
				queue.consumers,
				queue.durable,
				queue.auto_delete,
				queue.exclusive,
				queue.memory,
				queue.arguments
			);
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to get queue details from RabbitMQ Management API: {ex.Message}", ex);
		}
	}

	public async Task<bool> AddQueueAsync(AddQueueRequest request, ServerCredential? credential = null)
	{
		try
		{
			var factory = RabbitMQConfigBuilder.BuildConnectionFactory(request.ServerUrl, credential);
			await using var connection = await factory.CreateConnectionAsync();
			await using var channel = await connection.CreateChannelAsync();

			await channel.QueueDeclareAsync(
				queue: request.Name,
				durable: request.Durable,
				exclusive: request.Exclusive,
				autoDelete: request.AutoDelete);

			// Audit log after successful creation
			var details = JsonSerializer.Serialize(new
			{
				queueName = request.Name,
				durable = request.Durable,
				autoDelete = request.AutoDelete,
				serverUrl = request.ServerUrl
			});
			await _auditLog.Audit(
				ActivityTypeEnum.AddRabbitMQQueue,
				$"RabbitMQ queue '{request.Name}' created on server '{request.ServerUrl}'",
				details);

			return true;
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to create queue: {ex.Message}", ex);
		}
	}

	public async Task<bool> DeleteQueueAsync(DeleteQueueRequest request, ServerCredential? credential = null)
	{
		try
		{
			// Audit log before deletion
			var details = JsonSerializer.Serialize(new
			{
				queueName = request.Name,
				serverUrl = request.ServerUrl
			});
			await _auditLog.Audit(
				ActivityTypeEnum.RemoveRabbitMQQueue,
				$"RabbitMQ queue '{request.Name}' deleted from server '{request.ServerUrl}'",
				details);

			var factory = RabbitMQConfigBuilder.BuildConnectionFactory(request.ServerUrl, credential);
			await using var connection = await factory.CreateConnectionAsync();
			await using var channel = await connection.CreateChannelAsync();

			await channel.QueueDeleteAsync(queue: request.Name);

			return true;
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to delete queue: {ex.Message}", ex);
		}
	}

	public async Task<bool> TestConnectionAsync(string serverUrl, ServerCredential credential)
	{
		try
		{
			var factory = RabbitMQConfigBuilder.BuildConnectionFactory(serverUrl, credential);
			await using var connection = await factory.CreateConnectionAsync();

			// If connection succeeds, test is successful
			return connection.IsOpen;
		}
		catch
		{
			return false;
		}
	}
}

using FastEndpoints;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.RabbitMQ.Interfaces;
using Nanuq.RabbitMQ.Requests;

namespace Nanuq.WebApi.Endpoints.RabbitMQ;

public class DeleteQueue : EndpointWithoutRequest<bool>
{
	private IRabbitMQManagerRepository rabbitMQManager;
	private IRabbitMqRepository rabbitMqRepository;
	private ICredentialRepository credentialRepository;

	public DeleteQueue(IRabbitMQManagerRepository rabbitMQManager,
					   IRabbitMqRepository rabbitMqRepository,
					   ICredentialRepository credentialRepository)
	{
		this.rabbitMQManager = rabbitMQManager;
		this.rabbitMqRepository = rabbitMqRepository;
		this.credentialRepository = credentialRepository;
	}

	public override void Configure()
	{
		Delete("/rabbitmq/queue/{serverUrl}/{name}");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var serverUrl = Route<string>("serverUrl", isRequired: true);
		var name = Route<string>("name", isRequired: true);

		// Auto-detect credentials
		ServerCredential? credential = null;
		var rabbitMQServers = await rabbitMqRepository.GetAll();
		var rabbitMQServer = rabbitMQServers.FirstOrDefault(s => s.ServerUrl == serverUrl);

		if (rabbitMQServer != null)
		{
			credential = await credentialRepository.GetByServerAsync(rabbitMQServer.Id, ServerType.RabbitMQ);

			if (credential != null)
			{
				await credentialRepository.UpdateLastUsedAsync(credential.Id);
			}
		}

		var request = new DeleteQueueRequest(serverUrl!, name!);
		var result = await rabbitMQManager.DeleteQueueAsync(request, credential);
		await Send.OkAsync(result, ct);
	}
}

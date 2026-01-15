using FastEndpoints;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.RabbitMQ.Entities;
using Nanuq.RabbitMQ.Interfaces;

namespace Nanuq.WebApi.Endpoints.RabbitMQ;

public class GetQueueDetails : EndpointWithoutRequest<QueueDetails>
{
	private IRabbitMQManagerRepository rabbitMQManager;
	private IRabbitMqRepository rabbitMqRepository;
	private ICredentialRepository credentialRepository;

	public GetQueueDetails(IRabbitMQManagerRepository rabbitMQManager,
						   IRabbitMqRepository rabbitMqRepository,
						   ICredentialRepository credentialRepository)
	{
		this.rabbitMQManager = rabbitMQManager;
		this.rabbitMqRepository = rabbitMqRepository;
		this.credentialRepository = credentialRepository;
	}

	public override void Configure()
	{
		Get("/rabbitmq/queue/{serverUrl}/{queueName}");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var serverUrl = Route<string>("serverUrl", isRequired: true);
		var queueName = Route<string>("queueName", isRequired: true);

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

		var queueDetails = await rabbitMQManager.GetQueueDetailsAsync(serverUrl!, queueName!, credential);
		await Send.OkAsync(queueDetails, ct);
	}
}

using FastEndpoints;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.RabbitMQ.Entities;
using Nanuq.RabbitMQ.Interfaces;

namespace Nanuq.WebApi.Endpoints.RabbitMQ;

public class GetExchanges : EndpointWithoutRequest<IEnumerable<Exchange>>
{
	private IRabbitMQManagerRepository rabbitMQManager;
	private IRabbitMqRepository rabbitMqRepository;
	private ICredentialRepository credentialRepository;

	public GetExchanges(IRabbitMQManagerRepository rabbitMQManager,
						IRabbitMqRepository rabbitMqRepository,
						ICredentialRepository credentialRepository)
	{
		this.rabbitMQManager = rabbitMQManager;
		this.rabbitMqRepository = rabbitMqRepository;
		this.credentialRepository = credentialRepository;
	}

	public override void Configure()
	{
		Get("/rabbitmq/exchanges/{serverUrl}");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var serverUrl = Route<string>("serverUrl", isRequired: true);

		// Auto-detect credentials (Phase 5 pattern)
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

		var exchanges = await rabbitMQManager.GetExchangesAsync(serverUrl!, credential);
		await Send.OkAsync(exchanges, ct);
	}
}

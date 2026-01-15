using FastEndpoints;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.RabbitMQ.Interfaces;
using Nanuq.RabbitMQ.Requests;

namespace Nanuq.WebApi.Endpoints.RabbitMQ;

public class AddExchange : Endpoint<AddExchangeRequest, bool>
{
	private IRabbitMQManagerRepository rabbitMQManager;
	private IRabbitMqRepository rabbitMqRepository;
	private ICredentialRepository credentialRepository;

	public AddExchange(IRabbitMQManagerRepository rabbitMQManager,
					   IRabbitMqRepository rabbitMqRepository,
					   ICredentialRepository credentialRepository)
	{
		this.rabbitMQManager = rabbitMQManager;
		this.rabbitMqRepository = rabbitMqRepository;
		this.credentialRepository = credentialRepository;
	}

	public override void Configure()
	{
		Post("/rabbitmq/exchange");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(AddExchangeRequest req, CancellationToken ct)
	{
		// Auto-detect credentials
		ServerCredential? credential = null;
		var rabbitMQServers = await rabbitMqRepository.GetAll();
		var rabbitMQServer = rabbitMQServers.FirstOrDefault(s => s.ServerUrl == req.ServerUrl);

		if (rabbitMQServer != null)
		{
			credential = await credentialRepository.GetByServerAsync(rabbitMQServer.Id, ServerType.RabbitMQ);

			if (credential != null)
			{
				await credentialRepository.UpdateLastUsedAsync(credential.Id);
			}
		}

		var result = await rabbitMQManager.AddExchangeAsync(req, credential);
		await Send.OkAsync(result, ct);
	}
}

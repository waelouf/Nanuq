using FastEndpoints;
using Nanuq.RabbitMQ.Interfaces;
using System.Runtime.CompilerServices;

namespace Nanuq.WebApi.Endpoints.RabbitMQ;

public class GetRabbitMQExchanges : EndpointWithoutRequest<bool>
{
	private IRabbitMQManagerRepository rabbitMQManager;

	public GetRabbitMQExchanges(IRabbitMQManagerRepository rabbitMQManager)
	{
		this.rabbitMQManager = rabbitMQManager;
	}

	public override void Configure()
	{
		Get("/rabbitmq/exchanges/{serverUrl}/{username}:{password}");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var serverUrl = Route<string>("serverUrl", isRequired: true);
		var username = Route<string>("username", isRequired: true);
		var password = Route<string>("password", isRequired: true);

		rabbitMQManager.GetConnection(serverUrl!, username!, password!);
		await SendOkAsync(ct);
	}

}

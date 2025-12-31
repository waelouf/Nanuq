using FastEndpoints;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Sqlite.RabbitMQ;

public class DeleteRabbitMQ : EndpointWithoutRequest<bool>
{
	private IRabbitMqRepository rabbitMqRepository;

	public DeleteRabbitMQ(IRabbitMqRepository rabbitMqRepository)
	{
		this.rabbitMqRepository = rabbitMqRepository;
	}

	public override void Configure()
	{
		Delete("/sqlite/rabbitmq/{id}");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var id = Route<int>("id");

		var deleted = await rabbitMqRepository.Delete(id);

		if (deleted)
		{
			await Send.OkAsync(deleted, ct);
		}
		else
		{
			await Send.NotFoundAsync(ct);
		}
	}
}

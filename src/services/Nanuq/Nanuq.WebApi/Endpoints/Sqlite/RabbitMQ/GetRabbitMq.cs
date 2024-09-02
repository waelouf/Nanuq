using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Sqlite.Repositories;

namespace Nanuq.WebApi.Endpoints.Sqlite.RabbitMQ;

public class GetRabbitMQ : EndpointWithoutRequest<RabbitMQRecord>
{
	private IRabbitMqRepository rabbitMqRepository;

	public GetRabbitMQ(IRabbitMqRepository rabbitMqRepo)
	{
		this.rabbitMqRepository = rabbitMqRepo;
	}

	public override void Configure()
	{
		Get("/sqlite/rabbitmq/{id}");
		AllowAnonymous();
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var id = Route<int>("id", isRequired: true);
		var redisRecord = await rabbitMqRepository.Get(id);
		if (redisRecord is null)
		{
			await SendNotFoundAsync(ct);
			return;
		}

		await SendOkAsync(redisRecord, ct);
	}
}

using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Requests;

namespace Nanuq.WebApi.Endpoints.Sqlite.RabbitMQ;

public class AddRabbitMQ : Endpoint<AddRabbitMQRecordRequest, int>
{
	private IRabbitMqRepository rabbitMqRepository;

	public AddRabbitMQ(IRabbitMqRepository rabbitMqRepo)
	{
		this.rabbitMqRepository = rabbitMqRepo;
	}

	public override void Configure()
	{
		Post("/sqlite/rabbitmq");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(AddRabbitMQRecordRequest req, CancellationToken ct)
	{
		var inserted = await rabbitMqRepository.Add(req.ToRecord());
		await SendOkAsync(inserted, ct);
	}
}

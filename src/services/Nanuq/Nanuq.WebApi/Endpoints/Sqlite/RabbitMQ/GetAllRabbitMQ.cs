using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;

namespace Nanuq.WebApi.Endpoints.Sqlite.RabbitMQ
{
	public class GetAllRabbitMQ : EndpointWithoutRequest<IEnumerable<RabbitMQRecord>>
	{
		private IRabbitMqRepository rabbitMqRepository;

		public GetAllRabbitMQ(IRabbitMqRepository rabbitMqRepo)
		{
			this.rabbitMqRepository = rabbitMqRepo;
		}

		public override void Configure()
		{
			Get("/sqlite/rabbitmq");
			AllowAnonymous();
		}

		public override async Task HandleAsync(CancellationToken ct)
		{
			var rabbitMqRecords = await rabbitMqRepository.GetAll();
			await SendOkAsync(rabbitMqRecords, ct);
		}
	}
}

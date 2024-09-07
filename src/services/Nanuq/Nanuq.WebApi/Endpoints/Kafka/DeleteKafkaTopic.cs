using FastEndpoints;
using Nanuq.Kafka.Interfaces;
using Nanuq.Kafka.Requests;

namespace Nanuq.WebApi.Endpoints.Kafka;

public class DeleteKafkaTopic : EndpointWithoutRequest<bool>
{
	private ITopicsRepository topicsRepository;

	public DeleteKafkaTopic(ITopicsRepository topicsRepository)
	{
		this.topicsRepository = topicsRepository;
	}

	public override void Configure()
	{
		Delete("/kafka/topic/{bootstrapServer}/{topicName}");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var server = Route<string>("bootstrapServer", isRequired: true);
		var topicName = Route<string>("topicName", isRequired: true);

		var req = new DeleteKafkaTopicRequest(server, topicName);

		var deleted = await topicsRepository.DeleteTopicAsync(req);
		if (deleted)
		{
			await SendOkAsync(deleted, ct);
		}
		else
		{
			await SendNotFoundAsync(ct);
		}
	}
}

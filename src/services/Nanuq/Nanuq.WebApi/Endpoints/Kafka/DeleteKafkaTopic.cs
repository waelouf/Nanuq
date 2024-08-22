using FastEndpoints;
using Nanuq.Kafka.Interfaces;
using Nanuq.Kafka.Requests;

namespace Nanuq.WebApi.Endpoints.Kafka;

public class DeleteKafkaTopic : Endpoint<DeleteKafkaTopicRequest, bool>
{
	private ITopicsRepository topicsRepository;

	public DeleteKafkaTopic(ITopicsRepository topicsRepository)
	{
		this.topicsRepository = topicsRepository;
	}

	public override void Configure()
	{
		Delete("/kafka/topic");
		AllowAnonymous();
	}

	public override async Task HandleAsync(DeleteKafkaTopicRequest req, CancellationToken ct)
	{
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

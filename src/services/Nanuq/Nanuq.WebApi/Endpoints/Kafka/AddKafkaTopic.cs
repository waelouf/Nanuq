using FastEndpoints;
using Nanuq.Kafka.Interfaces;
using Nanuq.Kafka.Requests;

namespace Nanuq.WebApi.Endpoints.Kafka;

public class AddKafkaTopic : Endpoint<AddKafkaTopicRequest, bool>
{
	private ITopicsRepository topicsRepository;

	public AddKafkaTopic(ITopicsRepository topicsRepository)
	{
		this.topicsRepository = topicsRepository;
	}

	public override void Configure()
	{
		Post("/kafka/topic");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(AddKafkaTopicRequest req, CancellationToken ct)
	{
		var added = await topicsRepository.AddTopicAsync(req);
		await Send.OkAsync(added, ct);
	}
}

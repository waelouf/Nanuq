using FastEndpoints;
using Nanuq.Kafka.Entities;
using Nanuq.Kafka.Interfaces;

namespace Nanuq.WebApi.Endpoints.Kafka;

public class GetTopicDetails : EndpointWithoutRequest<TopicDetails>
{
	private ITopicsRepository topicsRepository;

	public GetTopicDetails(ITopicsRepository topicsRepository)
	{
		this.topicsRepository = topicsRepository;
	}

	public override void Configure()
	{
		Get("/kafka/topic/{server}/{topicName}");
		AllowAnonymous();
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var server = Route<string>("server", isRequired: true);
		var topicName = Route<string>("topicName", isRequired: true);
		
		var topicDetails = await topicsRepository.GetTopicDetailsAsync(server!, topicName!);
		await SendOkAsync(topicDetails, ct);
	}
}

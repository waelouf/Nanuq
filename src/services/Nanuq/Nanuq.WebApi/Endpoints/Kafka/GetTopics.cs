using FastEndpoints;
using Nanuq.Kafka.Entities;
using Nanuq.Kafka.Interfaces;

namespace Nanuq.WebApi.Endpoints.Kafka;

public class GetTopics : EndpointWithoutRequest<IEnumerable<Topic>>
{
	private ITopicsRepository topicsRepository;

	public GetTopics(ITopicsRepository topicsRepository)
	{
		this.topicsRepository = topicsRepository;
	}

	public override void Configure()
	{
		Get("/kafka/topic/{server}");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var server = Route<string>("server", isRequired: true);
		var topics = await topicsRepository.GetTopicsAsync(server!);
		await Send.OkAsync(topics, ct);
	}
}

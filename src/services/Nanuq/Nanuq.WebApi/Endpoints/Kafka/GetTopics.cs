using FastEndpoints;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Kafka.Entities;
using Nanuq.Kafka.Interfaces;

namespace Nanuq.WebApi.Endpoints.Kafka;

public class GetTopics : EndpointWithoutRequest<IEnumerable<Topic>>
{
	private ITopicsRepository topicsRepository;
	private IKafkaRepository kafkaRepository;
	private ICredentialRepository credentialRepository;

	public GetTopics(ITopicsRepository topicsRepository, IKafkaRepository kafkaRepository, ICredentialRepository credentialRepository)
	{
		this.topicsRepository = topicsRepository;
		this.kafkaRepository = kafkaRepository;
		this.credentialRepository = credentialRepository;
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

		// Try to get credentials for this server
		ServerCredential? credential = null;
		var kafkaServers = await kafkaRepository.GetAll();
		var kafkaServer = kafkaServers.FirstOrDefault(s => s.BootstrapServer == server);

		if (kafkaServer != null)
		{
			credential = await credentialRepository.GetByServerAsync(kafkaServer.Id, ServerType.Kafka);

			if (credential != null)
			{
				await credentialRepository.UpdateLastUsedAsync(credential.Id);
			}
		}

		var topics = await topicsRepository.GetTopicsAsync(server!, credential);
		await Send.OkAsync(topics, ct);
	}
}

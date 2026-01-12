using FastEndpoints;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Kafka.Interfaces;
using Nanuq.Kafka.Requests;

namespace Nanuq.WebApi.Endpoints.Kafka;

public class AddKafkaTopic : Endpoint<AddKafkaTopicRequest, bool>
{
	private ITopicsRepository topicsRepository;
	private IKafkaRepository kafkaRepository;
	private ICredentialRepository credentialRepository;

	public AddKafkaTopic(ITopicsRepository topicsRepository, IKafkaRepository kafkaRepository, ICredentialRepository credentialRepository)
	{
		this.topicsRepository = topicsRepository;
		this.kafkaRepository = kafkaRepository;
		this.credentialRepository = credentialRepository;
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
		// Try to get credentials for this server
		ServerCredential? credential = null;
		var kafkaServers = await kafkaRepository.GetAll();
		var kafkaServer = kafkaServers.FirstOrDefault(s => s.BootstrapServer == req.BootstrapServers);

		if (kafkaServer != null)
		{
			credential = await credentialRepository.GetByServerAsync(kafkaServer.Id, ServerType.Kafka);

			if (credential != null)
			{
				await credentialRepository.UpdateLastUsedAsync(credential.Id);
			}
		}

		var added = await topicsRepository.AddTopicAsync(req, credential);
		await Send.OkAsync(added, ct);
	}
}

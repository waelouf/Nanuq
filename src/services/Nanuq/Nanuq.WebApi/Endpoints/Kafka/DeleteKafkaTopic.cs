using FastEndpoints;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Kafka.Interfaces;
using Nanuq.Kafka.Requests;

namespace Nanuq.WebApi.Endpoints.Kafka;

public class DeleteKafkaTopic : EndpointWithoutRequest<bool>
{
	private ITopicsRepository topicsRepository;
	private IKafkaRepository kafkaRepository;
	private ICredentialRepository credentialRepository;

	public DeleteKafkaTopic(ITopicsRepository topicsRepository, IKafkaRepository kafkaRepository, ICredentialRepository credentialRepository)
	{
		this.topicsRepository = topicsRepository;
		this.kafkaRepository = kafkaRepository;
		this.credentialRepository = credentialRepository;
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

		var req = new DeleteKafkaTopicRequest(server, topicName);

		var deleted = await topicsRepository.DeleteTopicAsync(req, credential);
		if (deleted)
		{
			await Send.OkAsync(deleted, ct);
		}
		else
		{
			await Send.NotFoundAsync(ct);
		}
	}
}

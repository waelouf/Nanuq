using FastEndpoints;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Redis.Interfaces;
using Nanuq.Redis.Requests;

namespace Nanuq.WebApi.Endpoints.Redis;

public class PopListElement : Endpoint<PopListElementRequest, string?>
{
	private IRedisManagerRepository redisManager;
	private IRedisRepository redisRepository;
	private ICredentialRepository credentialRepository;

	public PopListElement(IRedisManagerRepository redisManager, IRedisRepository redisRepository, ICredentialRepository credentialRepository)
	{
		this.redisManager = redisManager;
		this.redisRepository = redisRepository;
		this.credentialRepository = credentialRepository;
	}

	public override void Configure()
	{
		Post("/redis/list/pop");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(PopListElementRequest req, CancellationToken ct)
	{
		// Try to get credentials for this server
		ServerCredential? credential = null;
		var redisServers = await redisRepository.GetAll();
		var redisServer = redisServers.FirstOrDefault(s => s.ServerUrl == req.ServerUrl);

		if (redisServer != null)
		{
			credential = await credentialRepository.GetByServerAsync(redisServer.Id, ServerType.Redis);

			if (credential != null)
			{
				await credentialRepository.UpdateLastUsedAsync(credential.Id);
			}
		}

		var value = await redisManager.PopListElementAsync(req.ServerUrl, req.Database, req.Key, req.PopLeft, credential);
		await Send.OkAsync(value, ct);
	}
}

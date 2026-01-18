using FastEndpoints;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Redis.Interfaces;

namespace Nanuq.WebApi.Endpoints.Redis;

public class GetSortedSetMembers : EndpointWithoutRequest<List<(string member, double score)>>
{
	private IRedisManagerRepository redisManager;
	private IRedisRepository redisRepository;
	private ICredentialRepository credentialRepository;

	public GetSortedSetMembers(IRedisManagerRepository redisManager, IRedisRepository redisRepository, ICredentialRepository credentialRepository)
	{
		this.redisManager = redisManager;
		this.redisRepository = redisRepository;
		this.credentialRepository = credentialRepository;
	}

	public override void Configure()
	{
		Get("/redis/sortedset/{server}/{database}/{key}");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var server = Route<string>("server", isRequired: true);
		var database = Route<int>("database", isRequired: true);
		var key = Route<string>("key", isRequired: true);

		// Try to get credentials for this server
		ServerCredential? credential = null;
		var redisServers = await redisRepository.GetAll();
		var redisServer = redisServers.FirstOrDefault(s => s.ServerUrl == server);

		if (redisServer != null)
		{
			credential = await credentialRepository.GetByServerAsync(redisServer.Id, ServerType.Redis);

			if (credential != null)
			{
				await credentialRepository.UpdateLastUsedAsync(credential.Id);
			}
		}

		var members = await redisManager.GetSortedSetMembersAsync(server!, database, key!, true, credential);
		await Send.OkAsync(members, ct);
	}
}

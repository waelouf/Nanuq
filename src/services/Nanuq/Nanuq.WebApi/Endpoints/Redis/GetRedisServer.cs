using FastEndpoints;
using Microsoft.Extensions.Logging;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Redis.Entities;
using Nanuq.Redis.Interfaces;

namespace Nanuq.WebApi.Endpoints.Redis;

public class GetRedisServer : EndpointWithoutRequest<ServerDetails>
{
	private IRedisManagerRepository redisManager;
	private IRedisRepository redisRepository;
	private ICredentialRepository credentialRepository;
	private ILogger<GetRedisServer> logger;

	public GetRedisServer(IRedisManagerRepository redisManager, IRedisRepository redisRepository, ICredentialRepository credentialRepository, ILogger<GetRedisServer> logger)
	{
		this.redisManager = redisManager;
		this.redisRepository = redisRepository;
		this.credentialRepository = credentialRepository;
		this.logger = logger;
	}

	public override void Configure()
	{
		Get("/redis/{server}");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var server = Route<string>("server", isRequired: true);
		logger.LogInformation("[ENDPOINT-DEBUG] GetRedisServer called for server: {Server}", server);

		// Try to get credentials for this server
		ServerCredential? credential = null;
		var redisServers = await redisRepository.GetAll();
		logger.LogInformation("[ENDPOINT-DEBUG] Found {Count} Redis servers in database", redisServers.Count());

		var redisServer = redisServers.FirstOrDefault(s => s.ServerUrl == server);

		if (redisServer != null)
		{
			logger.LogInformation("[ENDPOINT-DEBUG] Server '{Server}' found in database (ID: {Id}), retrieving credentials...", server, redisServer.Id);
			credential = await credentialRepository.GetByServerAsync(redisServer.Id, ServerType.Redis);

			if (credential != null)
			{
				logger.LogInformation("[ENDPOINT-DEBUG] Credential retrieved successfully, updating LastUsedAt");
				await credentialRepository.UpdateLastUsedAsync(credential.Id);
			}
			else
			{
				logger.LogWarning("[ENDPOINT-DEBUG] Server '{Server}' found (ID: {Id}) but no credentials configured", server, redisServer.Id);
			}
		}
		else
		{
			logger.LogWarning("[ENDPOINT-DEBUG] Server '{Server}' NOT found in database. Available servers: {Servers}",
				server, string.Join(", ", redisServers.Select(s => s.ServerUrl)));
		}

		logger.LogInformation("[ENDPOINT-DEBUG] Calling redisManager.GetDatabases with credential: {HasCredential}", credential != null ? "YES" : "NO");
		var redisDetails = redisManager.GetDatabases(server!, credential);
		await Send.OkAsync(redisDetails, ct);
	}
}

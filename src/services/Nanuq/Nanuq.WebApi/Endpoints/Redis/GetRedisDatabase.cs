using FastEndpoints;
using Nanuq.Redis.Entities;
using Nanuq.Redis.Interfaces;

namespace Nanuq.WebApi.Endpoints.Redis;

public class GetRedisDatabase : EndpointWithoutRequest<DatabaseDetails>
{
	private IRedisManagerRepository redisManager;

	public GetRedisDatabase(IRedisManagerRepository redisManager)
	{
		this.redisManager = redisManager;
	}

	public override void Configure()
	{
		Get("/redis/{server}/{database}");
		AllowAnonymous();
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var server = Route<string>("server", isRequired: true);
		var database = Route<int>("database", isRequired: true); 

		var databaseDetails = redisManager.GetDatabase(server, database);
		await SendOkAsync(databaseDetails, ct);
	}
}

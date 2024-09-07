using FastEndpoints;
using Nanuq.Redis.Interfaces;

namespace Nanuq.WebApi.Endpoints.Redis;

public class InvalidateRedisCache : EndpointWithoutRequest<bool>
{
	private IRedisManagerRepository redisManager;

	public InvalidateRedisCache(IRedisManagerRepository redisManager)
	{
		this.redisManager = redisManager;
	}

	public override void Configure()
	{
		Delete("/redis/string/{server}/{database}/{key}");
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

		var deleted = await redisManager.InvalidateCache(server!, database, key!);
		await SendOkAsync(deleted, ct);
	}
}

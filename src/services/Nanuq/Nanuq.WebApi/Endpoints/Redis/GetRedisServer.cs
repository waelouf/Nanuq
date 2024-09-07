using FastEndpoints;
using Nanuq.Redis.Entities;
using Nanuq.Redis.Interfaces;

namespace Nanuq.WebApi.Endpoints.Redis;

public class GetRedisServer : EndpointWithoutRequest<ServerDetails>
{
	private IRedisManagerRepository redisManager;

	public GetRedisServer(IRedisManagerRepository redisManager)
	{
		this.redisManager = redisManager;
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
		var redisDetails = redisManager.GetDatabases(server!);
		await SendOkAsync(redisDetails, ct);
	}
}

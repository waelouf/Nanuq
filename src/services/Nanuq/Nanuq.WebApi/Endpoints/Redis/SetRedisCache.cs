using FastEndpoints;
using Nanuq.Redis.Interfaces;
using Nanuq.Redis.Requests;

namespace Nanuq.WebApi.Endpoints.Redis;

public class SetRedisCache : Endpoint<SetStringCacheRequest, bool>
{
	private IRedisManagerRepository redisManager;

	public SetRedisCache(IRedisManagerRepository redisManager)
	{
		this.redisManager = redisManager;
	}

	public override void Configure()
	{
		Post("/redis/string/");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(SetStringCacheRequest req, CancellationToken ct)
	{
		var added = await redisManager.SetStringCache(req.ServerUrl, req.Database, req.Key, req.Value, req.TtlMilliseconds);
		await Send.OkAsync(added, ct);
	}
}

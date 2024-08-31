using FastEndpoints;
using Nanuq.Redis.Interfaces;

namespace Nanuq.WebApi.Endpoints.Redis
{
	public class GetRedisCache : EndpointWithoutRequest<string?>
	{
		private IRedisManagerRepository redisManager;

		public GetRedisCache(IRedisManagerRepository redisManager)
		{
			this.redisManager = redisManager;
		}

		public override void Configure()
		{
			Get("/redis/string/{server}/{database}/{key}");
			AllowAnonymous();
		}

		public async override Task HandleAsync(CancellationToken ct)
		{
			var server = Route<string>("server", isRequired: true);
			var database = Route<int>("database", isRequired: true);
			var key = Route<string>("key", isRequired: true);

			var val = await redisManager.GetStringCache(server!, database, key!);
			
			if(val == null)
			{
				await SendNotFoundAsync(ct);
			}
			else
			{
				await SendOkAsync(val, ct);
			}
			
		}
	}
}

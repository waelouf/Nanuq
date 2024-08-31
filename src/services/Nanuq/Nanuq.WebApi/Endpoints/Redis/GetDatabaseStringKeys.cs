using FastEndpoints;
using Nanuq.Redis.Interfaces;

namespace Nanuq.WebApi.Endpoints.Redis
{
	public class GetDatabaseStringKeys : EndpointWithoutRequest<List<string>>
	{
		private IRedisManagerRepository redisManager;

		public GetDatabaseStringKeys(IRedisManagerRepository redisManager)
		{
			this.redisManager = redisManager;
		}

		public override void Configure()
		{
			Get("/redis/string/{server}/{database}");
			AllowAnonymous();
		}

		public override async Task HandleAsync(CancellationToken ct)
		{
			var server = Route<string>("server", isRequired: true);
			var database = Route<int>("database", isRequired: true);

			var keys = redisManager.GetAllDatabaseStringKeys(server, database);
			await SendOkAsync(keys, ct);
		}
	}
}

using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;

namespace Nanuq.WebApi.Endpoints.Sqlite.Redis;

public class GetAllRedis : EndpointWithoutRequest<IEnumerable<RedisRecord>>
{
    private IRedisRepository redisRepository;

    public GetAllRedis(IRedisRepository redisRepository)
    {
        this.redisRepository = redisRepository;
    }

    public override void Configure()
    {
        Get("/sqlite/redis");
        AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader()));
	}

    public override async Task HandleAsync(CancellationToken ct)
    {
        var redisRecords = await redisRepository.GetAll();
        await SendOkAsync(redisRecords, ct);
    }
}

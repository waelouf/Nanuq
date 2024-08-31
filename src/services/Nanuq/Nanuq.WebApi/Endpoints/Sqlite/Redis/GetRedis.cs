using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;

namespace Nanuq.WebApi.Endpoints.Sqlite.Redis;

public class GetRedis : EndpointWithoutRequest<RedisRecord>
{
    private IRedisRepository redisRepository;

    public GetRedis(IRedisRepository redisRepository)
    {
        this.redisRepository = redisRepository;
    }

    public override void Configure()
    {
        Get("/sqlite/redis/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id", isRequired: true);
        var redisRecord = await redisRepository.Get(id);
        if (redisRecord is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(redisRecord, ct);
    }
}

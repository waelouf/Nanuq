using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Requests;

namespace Nanuq.WebApi.Endpoints.Sqlite.Redis;

public class AddRedis : Endpoint<AddRedisRecordRequest, int>
{
    private IRedisRepository redisRepository;

    public AddRedis(IRedisRepository redisRepository)
    {
        this.redisRepository = redisRepository;
    }

    public override void Configure()
    {
        Post("/sqlite/redis");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddRedisRecordRequest req, CancellationToken ct)
    {
        var inserted = await redisRepository.Add(req.ToRecord());
        await SendOkAsync(inserted, ct);
    }
}

using FastEndpoints;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Sqlite.Redis;

public class DeleteRedis : EndpointWithoutRequest<bool>
{
    private IRedisRepository redisRepository;

    public DeleteRedis(IRedisRepository redisRepository)
    {
        this.redisRepository = redisRepository;
    }

    public override void Configure()
    {
        Delete("/sqlite/redis/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var deleted = await redisRepository.Delete(id);

        if (deleted)
        {
            await SendOkAsync(deleted, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}

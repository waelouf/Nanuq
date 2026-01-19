using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;

namespace Nanuq.WebApi.Endpoints.Sqlite.AWS;

/// <summary>
/// Gets a specific AWS server configuration by ID
/// </summary>
public class GetAwsServer : EndpointWithoutRequest<AwsRecord>
{
    private IAwsRepository awsRepository;

    public GetAwsServer(IAwsRepository awsRepository)
    {
        this.awsRepository = awsRepository;
    }

    public override void Configure()
    {
        Get("/sqlite/aws/{id}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var awsRecord = await awsRepository.Get(id);

        if (awsRecord == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(awsRecord, ct);
    }
}

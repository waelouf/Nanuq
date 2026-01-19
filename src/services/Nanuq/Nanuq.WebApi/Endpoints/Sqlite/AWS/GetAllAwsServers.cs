using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;

namespace Nanuq.WebApi.Endpoints.Sqlite.AWS;

/// <summary>
/// Gets all AWS server configurations
/// </summary>
public class GetAllAwsServers : EndpointWithoutRequest<IEnumerable<AwsRecord>>
{
    private IAwsRepository awsRepository;

    public GetAllAwsServers(IAwsRepository awsRepository)
    {
        this.awsRepository = awsRepository;
    }

    public override void Configure()
    {
        Get("/sqlite/aws");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var awsRecords = await awsRepository.GetAll();
        await Send.OkAsync(awsRecords, ct);
    }
}

using FastEndpoints;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Sqlite.AWS;

/// <summary>
/// Deletes an AWS server configuration
/// </summary>
public class DeleteAwsServer : EndpointWithoutRequest<bool>
{
    private IAwsRepository awsRepository;

    public DeleteAwsServer(IAwsRepository awsRepository)
    {
        this.awsRepository = awsRepository;
    }

    public override void Configure()
    {
        Delete("/sqlite/aws/{id}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var deleted = await awsRepository.Delete(id);
        await Send.OkAsync(deleted, ct);
    }
}

using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Requests;

namespace Nanuq.WebApi.Endpoints.Sqlite.AWS;

/// <summary>
/// Adds a new AWS server configuration
/// </summary>
public class AddAwsServer : Endpoint<AddAwsRecordRequest, int>
{
    private IAwsRepository awsRepository;

    public AddAwsServer(IAwsRepository awsRepository)
    {
        this.awsRepository = awsRepository;
    }

    public override void Configure()
    {
        Post("/sqlite/aws");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(AddAwsRecordRequest req, CancellationToken ct)
    {
        var inserted = await awsRepository.Add(req.ToRecord());
        await Send.OkAsync(inserted, ct);
    }
}

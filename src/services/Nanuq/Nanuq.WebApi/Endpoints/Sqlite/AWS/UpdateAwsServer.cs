using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Common.Requests;

namespace Nanuq.WebApi.Endpoints.Sqlite.AWS;

/// <summary>
/// Updates an existing AWS server configuration
/// </summary>
public class UpdateAwsServer : Endpoint<UpdateAwsRecordRequest, bool>
{
    private IAwsRepository awsRepository;

    public UpdateAwsServer(IAwsRepository awsRepository)
    {
        this.awsRepository = awsRepository;
    }

    public override void Configure()
    {
        Put("/sqlite/aws/{id}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(UpdateAwsRecordRequest req, CancellationToken ct)
    {
        var id = Route<int>("id");

        var record = new AwsRecord
        {
            Id = id,
            Region = req.Region,
            Alias = req.Alias,
            Environment = req.Environment,
            ServiceType = req.ServiceType
        };

        var updated = await awsRepository.Update(record);
        await Send.OkAsync(updated, ct);
    }
}

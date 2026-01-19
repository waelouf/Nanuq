using FastEndpoints;
using Nanuq.Common.Records;
using Nanuq.Sqlite.Repositories;

namespace Nanuq.WebApi.Endpoints.Sqlite.Azure;

public class UpdateAzureServerRequest
{
    public int Id { get; set; }
    public string Alias { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Environment { get; set; } = "Development";
    public string ServiceType { get; set; } = "ServiceBus";
}

public class UpdateAzureServer : Endpoint<UpdateAzureServerRequest>
{
    private IAzureRepository azureRepository;

    public UpdateAzureServer(IAzureRepository azureRepository)
    {
        this.azureRepository = azureRepository;
    }

    public override void Configure()
    {
        Put("/sqlite/azure/{id}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(UpdateAzureServerRequest req, CancellationToken ct)
    {
        var id = Route<int>("id");

        var record = new AzureRecord
        {
            Id = id,
            Alias = req.Alias,
            Namespace = req.Namespace,
            Region = req.Region,
            Environment = req.Environment,
            ServiceType = req.ServiceType
        };

        var success = await azureRepository.Update(record);

        if (!success)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(ct);
    }
}

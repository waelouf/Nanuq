using FastEndpoints;
using Nanuq.Common.Records;
using Nanuq.Sqlite.Repositories;

namespace Nanuq.WebApi.Endpoints.Sqlite.Azure;

public class AddAzureServerRequest
{
    public string Alias { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Environment { get; set; } = "Development";
    public string ServiceType { get; set; } = "ServiceBus";
}

public class AddAzureServer : Endpoint<AddAzureServerRequest, AzureRecord>
{
    private IAzureRepository azureRepository;

    public AddAzureServer(IAzureRepository azureRepository)
    {
        this.azureRepository = azureRepository;
    }

    public override void Configure()
    {
        Post("/sqlite/azure");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(AddAzureServerRequest req, CancellationToken ct)
    {
        var record = new AzureRecord
        {
            Alias = req.Alias,
            Namespace = req.Namespace,
            Region = req.Region,
            Environment = req.Environment,
            ServiceType = req.ServiceType
        };

        var id = await azureRepository.Add(record);
        record.Id = id;

        await Send.OkAsync(record, ct);
    }
}

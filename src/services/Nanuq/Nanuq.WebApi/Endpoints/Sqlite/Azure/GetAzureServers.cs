using FastEndpoints;
using Nanuq.Common.Records;
using Nanuq.Sqlite.Repositories;

namespace Nanuq.WebApi.Endpoints.Sqlite.Azure;

public class GetAzureServers : EndpointWithoutRequest<IEnumerable<AzureRecord>>
{
    private IAzureRepository azureRepository;

    public GetAzureServers(IAzureRepository azureRepository)
    {
        this.azureRepository = azureRepository;
    }

    public override void Configure()
    {
        Get("/sqlite/azure");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var servers = await azureRepository.GetAll();
        await Send.OkAsync(servers, ct);
    }
}

using FastEndpoints;
using Nanuq.Common.Records;
using Nanuq.Sqlite.Repositories;

namespace Nanuq.WebApi.Endpoints.Sqlite.Azure;

public class GetAzureServer : EndpointWithoutRequest<AzureRecord>
{
    private IAzureRepository azureRepository;

    public GetAzureServer(IAzureRepository azureRepository)
    {
        this.azureRepository = azureRepository;
    }

    public override void Configure()
    {
        Get("/sqlite/azure/{id}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var server = await azureRepository.Get(id);

        if (server == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(server, ct);
    }
}

using FastEndpoints;
using Nanuq.Sqlite.Repositories;

namespace Nanuq.WebApi.Endpoints.Sqlite.Azure;

public class DeleteAzureServer : EndpointWithoutRequest
{
    private IAzureRepository azureRepository;

    public DeleteAzureServer(IAzureRepository azureRepository)
    {
        this.azureRepository = azureRepository;
    }

    public override void Configure()
    {
        Delete("/sqlite/azure/{id}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var success = await azureRepository.Delete(id);

        if (!success)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(ct);
    }
}

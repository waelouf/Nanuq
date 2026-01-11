using FastEndpoints;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Credentials;

public class DeleteCredential : EndpointWithoutRequest<bool>
{
    private readonly ICredentialRepository credentialRepository;

    public DeleteCredential(ICredentialRepository credentialRepository)
    {
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Delete("/credentials/{id}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id", isRequired: true);

        var deleted = await credentialRepository.DeleteAsync(id);
        await Send.OkAsync(deleted, ct);
    }
}

using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Requests;

namespace Nanuq.WebApi.Endpoints.Credentials;

public class UpdateCredential : Endpoint<UpdateCredentialRequest, bool>
{
    private readonly ICredentialRepository credentialRepository;

    public UpdateCredential(ICredentialRepository credentialRepository)
    {
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Put("/credentials/{id}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(UpdateCredentialRequest req, CancellationToken ct)
    {
        var id = Route<int>("id", isRequired: true);

        // Get existing credential
        var existingCredential = await credentialRepository.GetByIdAsync(id);
        if (existingCredential == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Update only provided fields
        if (!string.IsNullOrEmpty(req.Username))
        {
            existingCredential.Username = req.Username;
        }

        if (!string.IsNullOrEmpty(req.Password))
        {
            existingCredential.Password = req.Password;
        }

        if (req.AdditionalConfig != null)
        {
            existingCredential.AdditionalConfig = req.AdditionalConfig;
        }

        var updated = await credentialRepository.UpdateAsync(existingCredential);
        await Send.OkAsync(updated, ct);
    }
}

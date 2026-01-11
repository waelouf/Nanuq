using FastEndpoints;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Common.Requests;

namespace Nanuq.WebApi.Endpoints.Credentials;

public class AddCredential : Endpoint<AddCredentialRequest, int>
{
    private readonly ICredentialRepository credentialRepository;

    public AddCredential(ICredentialRepository credentialRepository)
    {
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Post("/credentials");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(AddCredentialRequest req, CancellationToken ct)
    {
        // Parse server type
        if (!Enum.TryParse<ServerType>(req.ServerType, ignoreCase: true, out var serverType))
        {
            ThrowError($"Invalid server type: {req.ServerType}");
        }

        var credential = new ServerCredential
        {
            ServerId = req.ServerId,
            ServerType = serverType.ToString(),
            Username = req.Username,
            Password = req.Password,
            AdditionalConfig = req.AdditionalConfig
        };

        var credentialId = await credentialRepository.AddAsync(credential);
        await Send.OkAsync(credentialId, ct);
    }
}

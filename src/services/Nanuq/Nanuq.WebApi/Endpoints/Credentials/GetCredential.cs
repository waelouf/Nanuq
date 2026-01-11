using FastEndpoints;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Credentials;

public class GetCredential : EndpointWithoutRequest
{
    private readonly ICredentialRepository credentialRepository;

    public GetCredential(ICredentialRepository credentialRepository)
    {
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Get("/credentials/{serverId}/{serverType}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var serverId = Route<int>("serverId", isRequired: true);
        var serverTypeStr = Route<string>("serverType", isRequired: true);

        // Parse server type
        if (!Enum.TryParse<ServerType>(serverTypeStr, ignoreCase: true, out var serverType))
        {
            ThrowError($"Invalid server type: {serverTypeStr}");
        }

        var credential = await credentialRepository.GetByServerAsync(serverId, serverType);

        if (credential == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Return metadata only - never expose passwords to frontend
        var metadata = new
        {
            id = credential.Id,
            serverId = credential.ServerId,
            serverType = credential.ServerType,
            hasCredentials = true,
            createdAt = credential.CreatedAt,
            lastUsedAt = credential.LastUsedAt
        };

        await Send.OkAsync(metadata, ct);
    }
}

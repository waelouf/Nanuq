using FastEndpoints;
using Nanuq.AWS.SNS.Interfaces;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.AWS.SNS;

/// <summary>
/// Unsubscribes from an SNS topic
/// </summary>
public class Unsubscribe : EndpointWithoutRequest<bool>
{
    private ISnsManagerRepository snsManager;
    private IAwsRepository awsRepository;
    private ICredentialRepository credentialRepository;

    public Unsubscribe(
        ISnsManagerRepository snsManager,
        IAwsRepository awsRepository,
        ICredentialRepository credentialRepository)
    {
        this.snsManager = snsManager;
        this.awsRepository = awsRepository;
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Delete("/aws/sns/subscription/{serverId}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var serverId = Route<int>("serverId");
        var subscriptionArn = Query<string>("subscriptionArn", isRequired: true);

        var awsServer = await awsRepository.Get(serverId);
        if (awsServer == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var credential = await credentialRepository.GetByServerAsync(serverId, Common.Enums.ServerType.AWS);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var result = await snsManager.UnsubscribeAsync(awsServer.Region, subscriptionArn!, credential);
        await Send.OkAsync(result, ct);
    }
}

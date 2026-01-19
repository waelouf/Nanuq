using FastEndpoints;
using Nanuq.AWS.SNS.Entities;
using Nanuq.AWS.SNS.Interfaces;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.AWS.SNS;

/// <summary>
/// Gets all SNS topics for a server
/// </summary>
public class GetTopics : EndpointWithoutRequest<IEnumerable<SnsTopic>>
{
    private ISnsManagerRepository snsManager;
    private IAwsRepository awsRepository;
    private ICredentialRepository credentialRepository;

    public GetTopics(
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
        Get("/aws/sns/topics/{serverId}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var serverId = Route<int>("serverId");

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

        var topics = await snsManager.GetTopicsAsync(awsServer.Region, credential);
        await Send.OkAsync(topics, ct);
    }
}

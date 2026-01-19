using FastEndpoints;
using Nanuq.AWS.SNS.Entities;
using Nanuq.AWS.SNS.Interfaces;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.AWS.SNS;

/// <summary>
/// Gets detailed information about a specific SNS topic
/// </summary>
public class GetTopicDetails : EndpointWithoutRequest<SnsTopicDetails>
{
    private ISnsManagerRepository snsManager;
    private IAwsRepository awsRepository;
    private ICredentialRepository credentialRepository;

    public GetTopicDetails(
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
        Get("/aws/sns/topic/details/{serverId}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var serverId = Route<int>("serverId");
        var topicArn = Query<string>("topicArn", isRequired: true);

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

        var details = await snsManager.GetTopicDetailsAsync(awsServer.Region, topicArn!, credential);
        await Send.OkAsync(details, ct);
    }
}

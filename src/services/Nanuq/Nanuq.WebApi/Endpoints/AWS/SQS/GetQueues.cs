using FastEndpoints;
using Nanuq.AWS.SQS.Entities;
using Nanuq.AWS.SQS.Interfaces;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.AWS.SQS;

/// <summary>
/// Gets all SQS queues for a server
/// </summary>
public class GetQueues : EndpointWithoutRequest<IEnumerable<SqsQueue>>
{
    private ISqsManagerRepository sqsManager;
    private IAwsRepository awsRepository;
    private ICredentialRepository credentialRepository;

    public GetQueues(
        ISqsManagerRepository sqsManager,
        IAwsRepository awsRepository,
        ICredentialRepository credentialRepository)
    {
        this.sqsManager = sqsManager;
        this.awsRepository = awsRepository;
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Get("/aws/sqs/queues/{serverId}");
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

        var queues = await sqsManager.GetQueuesAsync(awsServer.Region, credential);
        await Send.OkAsync(queues, ct);
    }
}

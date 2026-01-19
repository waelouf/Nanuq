using FastEndpoints;
using Nanuq.AWS.SQS.Entities;
using Nanuq.AWS.SQS.Interfaces;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.AWS.SQS;

/// <summary>
/// Gets detailed information about a specific SQS queue
/// </summary>
public class GetQueueDetails : EndpointWithoutRequest<SqsQueueDetails>
{
    private ISqsManagerRepository sqsManager;
    private IAwsRepository awsRepository;
    private ICredentialRepository credentialRepository;

    public GetQueueDetails(
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
        Get("/aws/sqs/queue/details/{serverId}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var serverId = Route<int>("serverId");
        var queueUrl = Query<string>("queueUrl", isRequired: true);

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

        var details = await sqsManager.GetQueueDetailsAsync(awsServer.Region, queueUrl!, credential);
        await Send.OkAsync(details, ct);
    }
}

using FastEndpoints;
using Nanuq.AWS.SQS.Interfaces;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.AWS.SQS;

/// <summary>
/// Deletes an SQS queue
/// </summary>
public class DeleteQueue : EndpointWithoutRequest<bool>
{
    private ISqsManagerRepository sqsManager;
    private IAwsRepository awsRepository;
    private ICredentialRepository credentialRepository;

    public DeleteQueue(
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
        Delete("/aws/sqs/queue/{serverId}");
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

        var result = await sqsManager.DeleteQueueAsync(awsServer.Region, queueUrl!, credential);
        await Send.OkAsync(result, ct);
    }
}

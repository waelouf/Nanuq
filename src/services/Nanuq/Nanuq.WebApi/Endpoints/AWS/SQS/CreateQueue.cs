using FastEndpoints;
using Nanuq.AWS.SQS.Interfaces;
using Nanuq.AWS.SQS.Requests;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.AWS.SQS;

/// <summary>
/// Request to create a new SQS queue
/// </summary>
public class CreateQueueEndpointRequest
{
    public int ServerId { get; set; }
    public string Region { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public int VisibilityTimeout { get; set; } = 30;
    public int MessageRetentionPeriod { get; set; } = 345600;
    public int MaximumMessageSize { get; set; } = 262144;
    public string? DeadLetterQueueArn { get; set; }
    public int MaxReceiveCount { get; set; } = 5;
    public bool IsFifo { get; set; } = false;
}

/// <summary>
/// Creates a new SQS queue
/// </summary>
public class CreateQueue : Endpoint<CreateQueueEndpointRequest, string>
{
    private ISqsManagerRepository sqsManager;
    private ICredentialRepository credentialRepository;

    public CreateQueue(
        ISqsManagerRepository sqsManager,
        ICredentialRepository credentialRepository)
    {
        this.sqsManager = sqsManager;
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Post("/aws/sqs/queue/create");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CreateQueueEndpointRequest req, CancellationToken ct)
    {
        var credential = await credentialRepository.GetByServerAsync(req.ServerId, Common.Enums.ServerType.AWS);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var request = new CreateQueueRequest(
            req.Region,
            req.QueueName,
            req.VisibilityTimeout,
            req.MessageRetentionPeriod,
            req.MaximumMessageSize,
            req.DeadLetterQueueArn,
            req.MaxReceiveCount,
            req.IsFifo
        );

        var queueUrl = await sqsManager.CreateQueueAsync(request, credential);
        await Send.OkAsync(queueUrl, ct);
    }
}

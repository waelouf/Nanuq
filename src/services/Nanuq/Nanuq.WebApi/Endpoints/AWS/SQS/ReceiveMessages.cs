using FastEndpoints;
using Nanuq.AWS.SQS.Entities;
using Nanuq.AWS.SQS.Interfaces;
using Nanuq.AWS.SQS.Requests;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.AWS.SQS;

/// <summary>
/// Request to receive messages from SQS
/// </summary>
public class ReceiveMessagesEndpointRequest
{
    public int ServerId { get; set; }
    public string Region { get; set; } = string.Empty;
    public string QueueUrl { get; set; } = string.Empty;
    public int MaxMessages { get; set; } = 10;
    public int VisibilityTimeout { get; set; } = 30;
    public int WaitTimeSeconds { get; set; } = 0;
}

/// <summary>
/// Receives messages from an SQS queue
/// </summary>
public class ReceiveMessages : Endpoint<ReceiveMessagesEndpointRequest, IEnumerable<SqsMessage>>
{
    private ISqsManagerRepository sqsManager;
    private ICredentialRepository credentialRepository;

    public ReceiveMessages(
        ISqsManagerRepository sqsManager,
        ICredentialRepository credentialRepository)
    {
        this.sqsManager = sqsManager;
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Post("/aws/sqs/message/receive");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(ReceiveMessagesEndpointRequest req, CancellationToken ct)
    {
        var credential = await credentialRepository.GetByServerAsync(req.ServerId, Common.Enums.ServerType.AWS);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var request = new ReceiveMessagesRequest(
            req.Region,
            req.QueueUrl,
            req.MaxMessages,
            req.VisibilityTimeout,
            req.WaitTimeSeconds
        );

        var messages = await sqsManager.ReceiveMessagesAsync(request, credential);
        await Send.OkAsync(messages, ct);
    }
}

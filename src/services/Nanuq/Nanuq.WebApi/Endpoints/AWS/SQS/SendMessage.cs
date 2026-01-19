using FastEndpoints;
using Nanuq.AWS.SQS.Interfaces;
using Nanuq.AWS.SQS.Requests;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.AWS.SQS;

/// <summary>
/// Request to send a message to SQS
/// </summary>
public class SendMessageEndpointRequest
{
    public int ServerId { get; set; }
    public string Region { get; set; } = string.Empty;
    public string QueueUrl { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
    public Dictionary<string, string>? MessageAttributes { get; set; }
    public int DelaySeconds { get; set; } = 0;
    public string? MessageGroupId { get; set; }
    public string? MessageDeduplicationId { get; set; }
}

/// <summary>
/// Sends a message to an SQS queue
/// </summary>
public class SendMessage : Endpoint<SendMessageEndpointRequest, bool>
{
    private ISqsManagerRepository sqsManager;
    private ICredentialRepository credentialRepository;

    public SendMessage(
        ISqsManagerRepository sqsManager,
        ICredentialRepository credentialRepository)
    {
        this.sqsManager = sqsManager;
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Post("/aws/sqs/message/send");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(SendMessageEndpointRequest req, CancellationToken ct)
    {
        var credential = await credentialRepository.GetByServerAsync(req.ServerId, Common.Enums.ServerType.AWS);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var request = new SendMessageRequest(
            req.Region,
            req.QueueUrl,
            req.MessageBody,
            req.MessageAttributes,
            req.DelaySeconds,
            req.MessageGroupId,
            req.MessageDeduplicationId
        );

        var result = await sqsManager.SendMessageAsync(request, credential);
        await Send.OkAsync(result, ct);
    }
}

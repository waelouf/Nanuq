using FastEndpoints;
using Nanuq.AWS.SNS.Interfaces;
using Nanuq.AWS.SNS.Requests;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.AWS.SNS;

/// <summary>
/// Request to publish a message to SNS
/// </summary>
public class PublishMessageEndpointRequest
{
    public int ServerId { get; set; }
    public string Region { get; set; } = string.Empty;
    public string TopicArn { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public Dictionary<string, string>? MessageAttributes { get; set; }
    public string? MessageGroupId { get; set; }
    public string? MessageDeduplicationId { get; set; }
}

/// <summary>
/// Publishes a message to an SNS topic
/// </summary>
public class PublishMessage : Endpoint<PublishMessageEndpointRequest, string>
{
    private ISnsManagerRepository snsManager;
    private ICredentialRepository credentialRepository;

    public PublishMessage(
        ISnsManagerRepository snsManager,
        ICredentialRepository credentialRepository)
    {
        this.snsManager = snsManager;
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Post("/aws/sns/message/publish");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(PublishMessageEndpointRequest req, CancellationToken ct)
    {
        var credential = await credentialRepository.GetByServerAsync(req.ServerId, Common.Enums.ServerType.AWS);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var request = new PublishMessageRequest(
            req.Region,
            req.TopicArn,
            req.Message,
            req.Subject,
            req.MessageAttributes,
            req.MessageGroupId,
            req.MessageDeduplicationId
        );

        var messageId = await snsManager.PublishMessageAsync(request, credential);
        await Send.OkAsync(messageId, ct);
    }
}

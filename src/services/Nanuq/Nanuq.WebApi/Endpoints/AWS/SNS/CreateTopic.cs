using FastEndpoints;
using Nanuq.AWS.SNS.Interfaces;
using Nanuq.AWS.SNS.Requests;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.AWS.SNS;

/// <summary>
/// Request to create a new SNS topic
/// </summary>
public class CreateTopicEndpointRequest
{
    public int ServerId { get; set; }
    public string Region { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool IsFifo { get; set; } = false;
}

/// <summary>
/// Creates a new SNS topic
/// </summary>
public class CreateTopic : Endpoint<CreateTopicEndpointRequest, string>
{
    private ISnsManagerRepository snsManager;
    private ICredentialRepository credentialRepository;

    public CreateTopic(
        ISnsManagerRepository snsManager,
        ICredentialRepository credentialRepository)
    {
        this.snsManager = snsManager;
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Post("/aws/sns/topic/create");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CreateTopicEndpointRequest req, CancellationToken ct)
    {
        var credential = await credentialRepository.GetByServerAsync(req.ServerId, Common.Enums.ServerType.AWS);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var request = new CreateTopicRequest(
            req.Region,
            req.TopicName,
            req.DisplayName,
            req.IsFifo
        );

        var topicArn = await snsManager.CreateTopicAsync(request, credential);
        await Send.OkAsync(topicArn, ct);
    }
}

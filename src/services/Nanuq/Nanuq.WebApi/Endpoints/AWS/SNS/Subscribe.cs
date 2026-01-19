using FastEndpoints;
using Nanuq.AWS.SNS.Interfaces;
using Nanuq.AWS.SNS.Requests;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.AWS.SNS;

/// <summary>
/// Request to subscribe to an SNS topic
/// </summary>
public class SubscribeEndpointRequest
{
    public int ServerId { get; set; }
    public string Region { get; set; } = string.Empty;
    public string TopicArn { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
}

/// <summary>
/// Subscribes an endpoint to an SNS topic
/// </summary>
public class Subscribe : Endpoint<SubscribeEndpointRequest, string>
{
    private ISnsManagerRepository snsManager;
    private ICredentialRepository credentialRepository;

    public Subscribe(
        ISnsManagerRepository snsManager,
        ICredentialRepository credentialRepository)
    {
        this.snsManager = snsManager;
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Post("/aws/sns/subscription/subscribe");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(SubscribeEndpointRequest req, CancellationToken ct)
    {
        var credential = await credentialRepository.GetByServerAsync(req.ServerId, Common.Enums.ServerType.AWS);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var request = new SubscribeRequest(
            req.Region,
            req.TopicArn,
            req.Protocol,
            req.Endpoint
        );

        var subscriptionArn = await snsManager.SubscribeAsync(request, credential);
        await Send.OkAsync(subscriptionArn, ct);
    }
}

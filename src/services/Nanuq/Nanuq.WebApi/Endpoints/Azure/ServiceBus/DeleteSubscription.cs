using FastEndpoints;
using Nanuq.Azure.ServiceBus.Repository;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Azure.ServiceBus;

public class DeleteSubscriptionRequest
{
    public int ServerId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public string SubscriptionName { get; set; } = string.Empty;
}

public class DeleteSubscription : Endpoint<DeleteSubscriptionRequest>
{
    private IServiceBusRepository serviceBusRepository;
    private ICredentialRepository credentialRepository;
    private IAuditLogRepository auditLog;

    public DeleteSubscription(
        IServiceBusRepository serviceBusRepository,
        ICredentialRepository credentialRepository,
        IAuditLogRepository auditLog)
    {
        this.serviceBusRepository = serviceBusRepository;
        this.credentialRepository = credentialRepository;
        this.auditLog = auditLog;
    }

    public override void Configure()
    {
        Delete("/azure/servicebus/subscription");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(DeleteSubscriptionRequest req, CancellationToken ct)
    {
        var credential = await credentialRepository.GetByServerAsync(req.ServerId, Common.Enums.ServerType.Azure);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var connectionString = credential.Password!;
        await serviceBusRepository.DeleteSubscriptionAsync(connectionString, req.TopicName, req.SubscriptionName);

        // Audit logging
        var details = System.Text.Json.JsonSerializer.Serialize(new
        {
            serverId = req.ServerId,
            topicName = req.TopicName,
            subscriptionName = req.SubscriptionName
        });

        await auditLog.Audit(
            ActivityTypeEnum.RemoveAzureSubscription,
            $"Azure Service Bus subscription '{req.SubscriptionName}' deleted from topic '{req.TopicName}'",
            details
        );

        await Send.OkAsync(ct);
    }
}

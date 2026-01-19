using FastEndpoints;
using Nanuq.Azure.ServiceBus.Repository;
using Nanuq.Azure.ServiceBus.Requests;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Azure.ServiceBus;

public class CreateSubscription : Endpoint<CreateSubscriptionRequest>
{
    private IServiceBusRepository serviceBusRepository;
    private ICredentialRepository credentialRepository;
    private IAuditLogRepository auditLog;

    public CreateSubscription(
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
        Post("/azure/servicebus/subscription");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CreateSubscriptionRequest req, CancellationToken ct)
    {
        var credential = await credentialRepository.GetByServerAsync(req.ServerId, Common.Enums.ServerType.Azure);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var connectionString = credential.Password!;
        await serviceBusRepository.CreateSubscriptionAsync(connectionString, req);

        // Audit logging
        var details = System.Text.Json.JsonSerializer.Serialize(new
        {
            serverId = req.ServerId,
            topicName = req.TopicName,
            subscriptionName = req.SubscriptionName
        });

        await auditLog.Audit(
            ActivityTypeEnum.AddAzureSubscription,
            $"Azure Service Bus subscription '{req.SubscriptionName}' created for topic '{req.TopicName}'",
            details
        );

        await Send.OkAsync(ct);
    }
}

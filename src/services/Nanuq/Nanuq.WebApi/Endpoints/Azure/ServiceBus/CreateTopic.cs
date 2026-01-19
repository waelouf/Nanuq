using FastEndpoints;
using Nanuq.Azure.ServiceBus.Repository;
using Nanuq.Azure.ServiceBus.Requests;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Azure.ServiceBus;

public class CreateTopic : Endpoint<CreateTopicRequest>
{
    private IServiceBusRepository serviceBusRepository;
    private ICredentialRepository credentialRepository;
    private IAuditLogRepository auditLog;

    public CreateTopic(
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
        Post("/azure/servicebus/topic");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CreateTopicRequest req, CancellationToken ct)
    {
        var credential = await credentialRepository.GetByServerAsync(req.ServerId, Common.Enums.ServerType.Azure);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        if (string.IsNullOrWhiteSpace(credential.Password))
        {
            ThrowError("Azure Service Bus connection string is not configured. Please add credentials for this server.");
        }

        var connectionString = credential.Password!;
        await serviceBusRepository.CreateTopicAsync(connectionString, req);

        // Audit logging
        var details = System.Text.Json.JsonSerializer.Serialize(new
        {
            serverId = req.ServerId,
            topicName = req.TopicName,
            maxSizeInMB = req.MaxSizeInMegabytes
        });

        await auditLog.Audit(
            ActivityTypeEnum.AddAzureServiceBusTopic,
            $"Azure Service Bus topic '{req.TopicName}' created",
            details
        );

        await Send.OkAsync(cancellation: ct);
    }
}

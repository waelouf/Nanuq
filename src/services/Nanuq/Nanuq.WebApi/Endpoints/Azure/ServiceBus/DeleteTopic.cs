using FastEndpoints;
using Nanuq.Azure.ServiceBus.Repository;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Azure.ServiceBus;

public class DeleteTopicRequest
{
    public int ServerId { get; set; }
    public string TopicName { get; set; } = string.Empty;
}

public class DeleteTopic : Endpoint<DeleteTopicRequest>
{
    private IServiceBusRepository serviceBusRepository;
    private ICredentialRepository credentialRepository;
    private IAuditLogRepository auditLog;

    public DeleteTopic(
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
        Delete("/azure/servicebus/topic");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(DeleteTopicRequest req, CancellationToken ct)
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
        await serviceBusRepository.DeleteTopicAsync(connectionString, req.TopicName);

        // Audit logging
        var details = System.Text.Json.JsonSerializer.Serialize(new
        {
            serverId = req.ServerId,
            topicName = req.TopicName
        });

        await auditLog.Audit(
            ActivityTypeEnum.RemoveAzureServiceBusTopic,
            $"Azure Service Bus topic '{req.TopicName}' deleted",
            details
        );

        await Send.OkAsync(cancellation: ct);
    }
}

using FastEndpoints;
using Nanuq.Azure.ServiceBus.Repository;
using Nanuq.Azure.ServiceBus.Requests;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Azure.ServiceBus;

public class PublishMessage : Endpoint<PublishMessageRequest>
{
    private IServiceBusRepository serviceBusRepository;
    private ICredentialRepository credentialRepository;
    private IAuditLogRepository auditLog;

    public PublishMessage(
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
        Post("/azure/servicebus/topic/message");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(PublishMessageRequest req, CancellationToken ct)
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
        await serviceBusRepository.PublishMessageAsync(connectionString, req);

        // Audit logging
        var details = System.Text.Json.JsonSerializer.Serialize(new
        {
            serverId = req.ServerId,
            topicName = req.TopicName,
            messageLength = req.MessageBody.Length
        });

        await auditLog.Audit(
            ActivityTypeEnum.PublishAzureServiceBusMessage,
            $"Message published to Azure Service Bus topic '{req.TopicName}'",
            details
        );

        await Send.OkAsync(cancellation: ct);
    }
}

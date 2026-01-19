using FastEndpoints;
using Nanuq.Azure.ServiceBus.Repository;
using Nanuq.Azure.ServiceBus.Requests;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Azure.ServiceBus;

public class SendMessage : Endpoint<SendMessageRequest>
{
    private IServiceBusRepository serviceBusRepository;
    private ICredentialRepository credentialRepository;
    private IAuditLogRepository auditLog;

    public SendMessage(
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
        Post("/azure/servicebus/queue/message");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(SendMessageRequest req, CancellationToken ct)
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
        await serviceBusRepository.SendMessageAsync(connectionString, req);

        // Audit logging
        var details = System.Text.Json.JsonSerializer.Serialize(new
        {
            serverId = req.ServerId,
            queueName = req.QueueName,
            messageLength = req.MessageBody.Length
        });

        await auditLog.Audit(
            ActivityTypeEnum.SendAzureServiceBusMessage,
            $"Message sent to Azure Service Bus queue '{req.QueueName}'",
            details
        );

        await Send.OkAsync(cancellation: ct);
    }
}

using FastEndpoints;
using Nanuq.Azure.ServiceBus.Repository;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Azure.ServiceBus;

public class DeleteQueueRequest
{
    public int ServerId { get; set; }
    public string QueueName { get; set; } = string.Empty;
}

public class DeleteQueue : Endpoint<DeleteQueueRequest>
{
    private IServiceBusRepository serviceBusRepository;
    private ICredentialRepository credentialRepository;
    private IAuditLogRepository auditLog;

    public DeleteQueue(
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
        Delete("/azure/servicebus/queue");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(DeleteQueueRequest req, CancellationToken ct)
    {
        var credential = await credentialRepository.GetByServerAsync(req.ServerId, Common.Enums.ServerType.Azure);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var connectionString = credential.Password!;
        await serviceBusRepository.DeleteQueueAsync(connectionString, req.QueueName);

        // Audit logging
        var details = System.Text.Json.JsonSerializer.Serialize(new
        {
            serverId = req.ServerId,
            queueName = req.QueueName
        });

        await auditLog.Audit(
            ActivityTypeEnum.RemoveAzureServiceBusQueue,
            $"Azure Service Bus queue '{req.QueueName}' deleted",
            details
        );

        await Send.OkAsync(ct);
    }
}

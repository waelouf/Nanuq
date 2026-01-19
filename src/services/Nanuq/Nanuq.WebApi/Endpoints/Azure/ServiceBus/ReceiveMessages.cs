using FastEndpoints;
using Nanuq.Azure.ServiceBus.Entities;
using Nanuq.Azure.ServiceBus.Repository;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Azure.ServiceBus;

public class ReceiveMessages : EndpointWithoutRequest<IEnumerable<ReceivedMessage>>
{
    private IServiceBusRepository serviceBusRepository;
    private ICredentialRepository credentialRepository;

    public ReceiveMessages(
        IServiceBusRepository serviceBusRepository,
        ICredentialRepository credentialRepository)
    {
        this.serviceBusRepository = serviceBusRepository;
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Get("/azure/servicebus/queue/{serverId}/{queueName}/messages");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var serverId = Route<int>("serverId");
        var queueName = Route<string>("queueName")!;

        var credential = await credentialRepository.GetByServerAsync(serverId, ServerType.Azure);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var connectionString = credential.Password!;
        var messages = await serviceBusRepository.ReceiveMessagesAsync(connectionString, queueName, 10);

        await Send.OkAsync(messages, ct);
    }
}

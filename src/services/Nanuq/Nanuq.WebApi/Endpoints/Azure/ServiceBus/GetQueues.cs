using FastEndpoints;
using Nanuq.Azure.ServiceBus.Entities;
using Nanuq.Azure.ServiceBus.Repository;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Azure.ServiceBus;

public class GetQueues : EndpointWithoutRequest<IEnumerable<ServiceBusQueue>>
{
    private IServiceBusRepository serviceBusRepository;
    private ICredentialRepository credentialRepository;

    public GetQueues(
        IServiceBusRepository serviceBusRepository,
        ICredentialRepository credentialRepository)
    {
        this.serviceBusRepository = serviceBusRepository;
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Get("/azure/servicebus/queues/{serverId}");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var serverId = Route<int>("serverId");

        var credential = await credentialRepository.GetByServerAsync(serverId, ServerType.Azure);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var connectionString = credential.Password!;
        var queues = await serviceBusRepository.GetQueuesAsync(connectionString);

        await Send.OkAsync(queues, ct);
    }
}

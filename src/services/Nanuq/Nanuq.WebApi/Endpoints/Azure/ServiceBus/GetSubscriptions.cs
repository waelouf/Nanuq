using FastEndpoints;
using Nanuq.Azure.ServiceBus.Entities;
using Nanuq.Azure.ServiceBus.Repository;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Azure.ServiceBus;

public class GetSubscriptions : EndpointWithoutRequest<IEnumerable<ServiceBusSubscription>>
{
    private IServiceBusRepository serviceBusRepository;
    private ICredentialRepository credentialRepository;

    public GetSubscriptions(
        IServiceBusRepository serviceBusRepository,
        ICredentialRepository credentialRepository)
    {
        this.serviceBusRepository = serviceBusRepository;
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Get("/azure/servicebus/topic/{serverId}/{topicName}/subscriptions");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var serverId = Route<int>("serverId");
        var topicName = Route<string>("topicName")!;

        var credential = await credentialRepository.GetByServerAsync(serverId, ServerType.Azure);
        if (credential == null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var connectionString = credential.Password!;
        var subscriptions = await serviceBusRepository.GetSubscriptionsAsync(connectionString, topicName);

        await Send.OkAsync(subscriptions, ct);
    }
}

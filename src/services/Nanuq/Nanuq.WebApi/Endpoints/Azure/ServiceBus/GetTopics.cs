using FastEndpoints;
using Nanuq.Azure.ServiceBus.Entities;
using Nanuq.Azure.ServiceBus.Repository;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Azure.ServiceBus;

public class GetTopics : EndpointWithoutRequest<IEnumerable<ServiceBusTopic>>
{
    private IServiceBusRepository serviceBusRepository;
    private ICredentialRepository credentialRepository;

    public GetTopics(
        IServiceBusRepository serviceBusRepository,
        ICredentialRepository credentialRepository)
    {
        this.serviceBusRepository = serviceBusRepository;
        this.credentialRepository = credentialRepository;
    }

    public override void Configure()
    {
        Get("/azure/servicebus/topics/{serverId}");
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

        if (string.IsNullOrWhiteSpace(credential.Password))
        {
            ThrowError("Azure Service Bus connection string is not configured. Please add credentials for this server.");
        }

        var connectionString = credential.Password!;
        var topics = await serviceBusRepository.GetTopicsAsync(connectionString);

        await Send.OkAsync(topics, ct);
    }
}

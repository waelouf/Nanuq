﻿using FastEndpoints;
using Nanuq.Sqlite.Interfaces;
using Nanuq.Sqlite.Records;

namespace Nanuq.WebApi.Endpoints.Sqlite.Kafka;

public class GetKafka : EndpointWithoutRequest<KafkaRecord>
{
    private IKafkaRepository kafkaRepository;

    public GetKafka(IKafkaRepository kafkaRepository)
    {
        this.kafkaRepository = kafkaRepository;
    }

    public override void Configure()
    {
        Get("/kafka/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id", isRequired: true);
        var kafkaRecord = await kafkaRepository.Get(id);
        if (kafkaRecord is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(kafkaRecord, ct);
    }
}
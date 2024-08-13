using FastEndpoints;
using Nanuq.Sqlite.Interfaces;
using Nanuq.Sqlite.Records;
using Nanuq.Sqlite.Requests;

namespace Nanuq.WebApi.Endpoints.Sqlite.Kafka;

public class AddKafka : Endpoint<AddKafkaRequest, int>
{
    private IKafkaRepository kafkaRepository;

    public AddKafka(IKafkaRepository kafkaRepository)
    {
        this.kafkaRepository = kafkaRepository;
    }

    public override void Configure()
    {
        Post("/kafka");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddKafkaRequest req, CancellationToken ct)
    {
        var inserted = await kafkaRepository.Add(req.ToRecord());
        await SendOkAsync(inserted, ct);
    }
}

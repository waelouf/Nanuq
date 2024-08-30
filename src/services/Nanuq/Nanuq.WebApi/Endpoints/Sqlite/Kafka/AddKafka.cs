using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Requests;

namespace Nanuq.WebApi.Endpoints.Sqlite.Kafka;

public class AddKafka : Endpoint<AddKafkaRecordRequest, int>
{
    private IKafkaRepository kafkaRepository;

    public AddKafka(IKafkaRepository kafkaRepository)
    {
        this.kafkaRepository = kafkaRepository;
    }

    public override void Configure()
    {
        Post("/sqlite/kafka");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddKafkaRecordRequest req, CancellationToken ct)
    {
        var inserted = await kafkaRepository.Add(req.ToRecord());
        await SendOkAsync(inserted, ct);
    }
}

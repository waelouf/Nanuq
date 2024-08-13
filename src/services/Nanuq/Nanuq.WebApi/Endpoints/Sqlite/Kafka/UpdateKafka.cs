using FastEndpoints;
using Nanuq.Sqlite.Interfaces;
using Nanuq.Sqlite.Records;
using Nanuq.Sqlite.Requests;

namespace Nanuq.WebApi.Endpoints.Sqlite.Kafka;

public class UpdateKafka : Endpoint<UpdateKafkaRequest, bool>
{
    private IKafkaRepository kafkaRepository;

    public UpdateKafka(IKafkaRepository kafkaRepository)
    {
        this.kafkaRepository = kafkaRepository;
    }

    public override void Configure()
    {
        Put("/kafka");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateKafkaRequest req, CancellationToken ct)
    {
        var updated = await kafkaRepository.Update(req.ToRecord());
        if (updated)
        {
            await SendOkAsync(updated, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}

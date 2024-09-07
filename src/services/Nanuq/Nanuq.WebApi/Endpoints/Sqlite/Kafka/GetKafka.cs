using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;

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
        Get("/sqlite/kafka/{id}");
        AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
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

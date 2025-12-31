using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;

namespace Nanuq.WebApi.Endpoints.Sqlite.Kafka;

public class GetAllKafka : EndpointWithoutRequest<IEnumerable<KafkaRecord>>
{
    private IKafkaRepository kafkaRepository;

    public GetAllKafka(IKafkaRepository kafkaRepository)
    {
        this.kafkaRepository = kafkaRepository;
    }

    public override void Configure()
    {
        Get("/sqlite/kafka");
        AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

    public override async Task HandleAsync(CancellationToken ct)
    {
        var kafkaRecords = await kafkaRepository.GetAll();
        await Send.OkAsync(kafkaRecords, ct);
    }
}

using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Requests;

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
        Put("/sqlite/kafka");
        AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

    public override async Task HandleAsync(UpdateKafkaRequest req, CancellationToken ct)
    {
        var updated = await kafkaRepository.Update(req.ToRecord());
        if (updated)
        {
            await Send.OkAsync(updated, ct);
        }
        else
        {
            await Send.NotFoundAsync(ct);
        }
    }
}

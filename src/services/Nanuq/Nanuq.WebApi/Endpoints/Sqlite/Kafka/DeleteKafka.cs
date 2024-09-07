using FastEndpoints;
using Nanuq.Common.Interfaces;

namespace Nanuq.WebApi.Endpoints.Sqlite.Kafka;

public class DeleteKafka : EndpointWithoutRequest<bool>
{
    private IKafkaRepository kafkaRepository;

    public DeleteKafka(IKafkaRepository kafkaRepository)
    {
        this.kafkaRepository = kafkaRepository;
    }

    public override void Configure()
    {
        Delete("/sqlite/kafka/{id}");
        AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()));
	}

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var deleted = await kafkaRepository.Delete(id);

        if (deleted)
        {
            await SendOkAsync(deleted, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}

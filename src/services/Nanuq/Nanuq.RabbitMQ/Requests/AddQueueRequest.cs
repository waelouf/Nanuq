namespace Nanuq.RabbitMQ.Requests;

public record AddQueueRequest(
    string ServerUrl,
    string Name
)
{
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public bool Exclusive { get; set; } = false;
}

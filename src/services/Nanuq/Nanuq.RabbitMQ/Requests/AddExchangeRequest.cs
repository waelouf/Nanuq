namespace Nanuq.RabbitMQ.Requests;

public record AddExchangeRequest(
    string ServerUrl,
    string Name,
    string Type  // direct, fanout, topic, headers
)
{
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
}

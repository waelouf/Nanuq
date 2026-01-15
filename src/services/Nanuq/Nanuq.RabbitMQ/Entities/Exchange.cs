namespace Nanuq.RabbitMQ.Entities;

public record Exchange(
    string Name,
    string Type,        // direct, fanout, topic, headers
    bool Durable,
    bool AutoDelete,
    IDictionary<string, object>? Arguments = null
);

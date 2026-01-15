namespace Nanuq.RabbitMQ.Entities;

public record QueueDetails(
    string Name,
    int MessageCount,
    int ConsumerCount,
    bool Durable,
    bool AutoDelete,
    bool Exclusive,
    long MemoryBytes,
    IDictionary<string, object>? Arguments
);

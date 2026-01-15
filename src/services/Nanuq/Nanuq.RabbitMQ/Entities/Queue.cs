namespace Nanuq.RabbitMQ.Entities;

public record Queue(
    string Name,
    int MessageCount,
    int ConsumerCount,
    bool Durable,
    bool AutoDelete,
    bool Exclusive
);

namespace Nanuq.RabbitMQ.Entities;

public record Binding(
    string Source,
    string Destination,
    string RoutingKey,
    string DestinationType  // queue or exchange
);

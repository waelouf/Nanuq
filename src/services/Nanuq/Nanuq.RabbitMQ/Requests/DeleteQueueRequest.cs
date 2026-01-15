namespace Nanuq.RabbitMQ.Requests;

public record DeleteQueueRequest(string ServerUrl, string Name);

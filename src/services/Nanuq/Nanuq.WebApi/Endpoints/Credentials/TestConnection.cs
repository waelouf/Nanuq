using Confluent.Kafka;
using Confluent.Kafka.Admin;
using FastEndpoints;
using Nanuq.Common.Enums;
using StackExchange.Redis;

namespace Nanuq.WebApi.Endpoints.Credentials;

public record TestConnectionRequest(
    int ServerId,
    string ServerType,
    string? Username,
    string? Password
);

public record TestConnectionResponse(bool Success, string Message);

public class TestConnection : Endpoint<TestConnectionRequest, TestConnectionResponse>
{
    public override void Configure()
    {
        Post("/credentials/test");
        AllowAnonymous();
        Options(b => b.RequireCors(x => x.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));
    }

    public override async Task HandleAsync(TestConnectionRequest req, CancellationToken ct)
    {
        // Parse server type
        if (!Enum.TryParse<Nanuq.Common.Enums.ServerType>(req.ServerType, ignoreCase: true, out var serverType))
        {
            await Send.OkAsync(new TestConnectionResponse(false, $"Invalid server type: {req.ServerType}"), ct);
            return;
        }

        try
        {
            var result = serverType switch
            {
                Nanuq.Common.Enums.ServerType.Kafka => await TestKafkaConnection(req.Username, req.Password),
                Nanuq.Common.Enums.ServerType.Redis => await TestRedisConnection(req.Username, req.Password),
                Nanuq.Common.Enums.ServerType.RabbitMQ => await TestRabbitMQConnection(req.Username, req.Password),
                _ => new TestConnectionResponse(false, "Unsupported server type")
            };

            await Send.OkAsync(result, ct);
        }
        catch (Exception ex)
        {
            await Send.OkAsync(new TestConnectionResponse(false, $"Connection test failed: {ex.Message}"), ct);
        }
    }

    private async Task<TestConnectionResponse> TestKafkaConnection(string? username, string? password)
    {
        // For test purposes, use a dummy bootstrap server
        // In a real implementation, this would need the actual server URL from the database
        var config = new AdminClientConfig
        {
            BootstrapServers = "localhost:9092" // This should come from the server record
        };

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            config.SecurityProtocol = SecurityProtocol.SaslPlaintext;
            config.SaslMechanism = SaslMechanism.Plain;
            config.SaslUsername = username;
            config.SaslPassword = password;
        }

        using var adminClient = new AdminClientBuilder(config).Build();
        try
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
            return await Task.FromResult(new TestConnectionResponse(true, "Kafka connection successful"));
        }
        catch (Exception ex)
        {
            return new TestConnectionResponse(false, $"Kafka connection failed: {ex.Message}");
        }
    }

    private async Task<TestConnectionResponse> TestRedisConnection(string? username, string? password)
    {
        var configOptions = new ConfigurationOptions
        {
            EndPoints = { "localhost:6379" }, // This should come from the server record
            ConnectTimeout = 5000
        };

        if (!string.IsNullOrEmpty(username))
        {
            configOptions.User = username;
        }

        if (!string.IsNullOrEmpty(password))
        {
            configOptions.Password = password;
        }

        try
        {
            using var redis = await ConnectionMultiplexer.ConnectAsync(configOptions);
            var db = redis.GetDatabase();
            await db.PingAsync();
            return new TestConnectionResponse(true, "Redis connection successful");
        }
        catch (Exception ex)
        {
            return new TestConnectionResponse(false, $"Redis connection failed: {ex.Message}");
        }
    }

    private async Task<TestConnectionResponse> TestRabbitMQConnection(string? username, string? password)
    {
        // RabbitMQ connection test placeholder
        // This would need the RabbitMQ.Client library and actual server URL
        return await Task.FromResult(new TestConnectionResponse(false, "RabbitMQ connection test not yet implemented"));
    }
}

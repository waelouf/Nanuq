using Confluent.Kafka;
using Confluent.Kafka.Admin;
using FastEndpoints;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.RabbitMQ.Helpers;
using StackExchange.Redis;

namespace Nanuq.WebApi.Endpoints.Credentials;

public record TestConnectionRequest(
    int ServerId,
    string ServerType,
    string? Username,
    string? Password,
    string? AdditionalConfig
);

public record TestConnectionResponse(bool Success, string Message);

public class TestConnection : Endpoint<TestConnectionRequest, TestConnectionResponse>
{
    private readonly IKafkaRepository _kafkaRepository;
    private readonly IRedisRepository _redisRepository;
    private readonly IRabbitMqRepository _rabbitMqRepository;

    public TestConnection(
        IKafkaRepository kafkaRepository,
        IRedisRepository redisRepository,
        IRabbitMqRepository rabbitMqRepository)
    {
        _kafkaRepository = kafkaRepository;
        _redisRepository = redisRepository;
        _rabbitMqRepository = rabbitMqRepository;
    }

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
                Nanuq.Common.Enums.ServerType.Kafka => await TestKafkaConnection(req.ServerId, req.Username, req.Password),
                Nanuq.Common.Enums.ServerType.Redis => await TestRedisConnection(req.ServerId, req.Username, req.Password),
                Nanuq.Common.Enums.ServerType.RabbitMQ => await TestRabbitMQConnection(req.ServerId, req.Username, req.Password),
                _ => new TestConnectionResponse(false, "Unsupported server type")
            };

            await Send.OkAsync(result, ct);
        }
        catch (Exception ex)
        {
            await Send.OkAsync(new TestConnectionResponse(false, $"Connection test failed: {ex.Message}"), ct);
        }
    }

    private async Task<TestConnectionResponse> TestKafkaConnection(int serverId, string? username, string? password)
    {
        try
        {
            var kafkaServer = await _kafkaRepository.Get(serverId);
            if (kafkaServer == null)
            {
                return new TestConnectionResponse(false, $"Kafka server with ID {serverId} not found");
            }

            var config = new AdminClientConfig
            {
                BootstrapServers = kafkaServer.BootstrapServer
            };

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                config.SecurityProtocol = SecurityProtocol.SaslPlaintext;
                config.SaslMechanism = SaslMechanism.Plain;
                config.SaslUsername = username;
                config.SaslPassword = password;
            }

            using var adminClient = new AdminClientBuilder(config).Build();
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
            return new TestConnectionResponse(true, "Kafka connection successful");
        }
        catch (Exception ex)
        {
            return new TestConnectionResponse(false, $"Kafka connection failed: {ex.Message}");
        }
    }

    private async Task<TestConnectionResponse> TestRedisConnection(int serverId, string? username, string? password)
    {
        try
        {
            var redisServer = await _redisRepository.Get(serverId);
            if (redisServer == null)
            {
                return new TestConnectionResponse(false, $"Redis server with ID {serverId} not found");
            }

            var configOptions = new ConfigurationOptions
            {
                EndPoints = { redisServer.ServerUrl },
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

    private async Task<TestConnectionResponse> TestRabbitMQConnection(int serverId, string? username, string? password)
    {
        try
        {
            var rabbitMqServer = await _rabbitMqRepository.Get(serverId);
            if (rabbitMqServer == null)
            {
                return new TestConnectionResponse(false, $"RabbitMQ server with ID {serverId} not found");
            }

            ServerCredential? credential = null;
            if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
            {
                credential = new ServerCredential
                {
                    Username = username,
                    Password = password
                };
            }

            var factory = RabbitMQConfigBuilder.BuildConnectionFactory(rabbitMqServer.ServerUrl, credential);
            await using var connection = await factory.CreateConnectionAsync();

            if (connection.IsOpen)
            {
                return new TestConnectionResponse(true, "RabbitMQ connection successful");
            }
            else
            {
                return new TestConnectionResponse(false, "RabbitMQ connection failed: Connection not open");
            }
        }
        catch (Exception ex)
        {
            return new TestConnectionResponse(false, $"RabbitMQ connection failed: {ex.Message}");
        }
    }
}

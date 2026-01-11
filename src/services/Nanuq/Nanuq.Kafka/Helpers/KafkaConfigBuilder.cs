using Confluent.Kafka;
using Nanuq.Common.Records;

namespace Nanuq.Kafka.Helpers;

public static class KafkaConfigBuilder
{
    /// <summary>
    /// Builds AdminClientConfig with optional credentials for SASL authentication
    /// </summary>
    public static AdminClientConfig BuildAdminConfig(
        string bootstrapServers,
        ServerCredential? credential = null)
    {
        var config = new AdminClientConfig
        {
            BootstrapServers = bootstrapServers
        };

        if (credential?.Username != null)
        {
            config.SecurityProtocol = SecurityProtocol.SaslPlaintext;
            config.SaslMechanism = SaslMechanism.Plain;
            config.SaslUsername = credential.Username;
            config.SaslPassword = credential.Password;
        }

        return config;
    }

    /// <summary>
    /// Builds ConsumerConfig with optional credentials for SASL authentication
    /// </summary>
    public static ConsumerConfig BuildConsumerConfig(
        string bootstrapServers,
        string groupId,
        ServerCredential? credential = null)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            EnableAutoCommit = false
        };

        if (credential?.Username != null)
        {
            config.SecurityProtocol = SecurityProtocol.SaslPlaintext;
            config.SaslMechanism = SaslMechanism.Plain;
            config.SaslUsername = credential.Username;
            config.SaslPassword = credential.Password;
        }

        return config;
    }
}

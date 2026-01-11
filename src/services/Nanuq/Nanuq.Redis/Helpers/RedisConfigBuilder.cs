using StackExchange.Redis;
using Nanuq.Common.Records;

namespace Nanuq.Redis.Helpers;

public static class RedisConfigBuilder
{
    /// <summary>
    /// Builds ConfigurationOptions with optional credentials for Redis authentication
    /// </summary>
    public static ConfigurationOptions BuildConfig(
        string serverUrl,
        ServerCredential? credential = null)
    {
        var config = new ConfigurationOptions
        {
            EndPoints = { serverUrl },
            AllowAdmin = true
        };

        if (credential != null)
        {
            // Redis 6+ ACL username support
            if (!string.IsNullOrEmpty(credential.Username))
            {
                config.User = credential.Username;
            }

            // Password authentication
            if (!string.IsNullOrEmpty(credential.Password))
            {
                config.Password = credential.Password;
            }
        }

        return config;
    }
}

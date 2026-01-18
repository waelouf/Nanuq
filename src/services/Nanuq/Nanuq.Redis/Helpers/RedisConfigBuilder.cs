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
            Console.WriteLine($"[DEBUG] RedisConfigBuilder: Building config for '{serverUrl}' - Username: {(string.IsNullOrEmpty(credential.Username) ? "EMPTY" : "SET")}, Password: {(string.IsNullOrEmpty(credential.Password) ? "EMPTY" : "SET")}");

            // Redis 6+ ACL username support
            if (!string.IsNullOrEmpty(credential.Username))
            {
                config.User = credential.Username;
                Console.WriteLine($"[DEBUG] RedisConfigBuilder: Set config.User = '{credential.Username}'");
            }

            // Password authentication
            if (!string.IsNullOrEmpty(credential.Password))
            {
                config.Password = credential.Password;
                Console.WriteLine($"[DEBUG] RedisConfigBuilder: Set config.Password (length: {credential.Password.Length})");
            }
        }
        else
        {
            Console.WriteLine($"[DEBUG] RedisConfigBuilder: Building config for '{serverUrl}' - NO CREDENTIAL PROVIDED");
        }

        return config;
    }
}

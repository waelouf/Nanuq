using Amazon.Runtime;
using Nanuq.Common.Records;
using System.Text.Json;

namespace Nanuq.AWS.Common;

/// <summary>
/// Helper class for building AWS credentials and region endpoints
/// </summary>
public static class AwsCredentialHelper
{
    /// <summary>
    /// Builds AWS credentials from ServerCredential
    /// </summary>
    /// <param name="credential">Server credential containing Access Key ID (Username) and Secret Access Key (Password)</param>
    /// <returns>AWS credentials object</returns>
    /// <exception cref="ArgumentNullException">Thrown when credential is null</exception>
    public static AWSCredentials BuildCredentials(ServerCredential? credential)
    {
        if (credential == null)
        {
            throw new ArgumentNullException(nameof(credential), "AWS credentials are required");
        }

        // Username = Access Key ID
        // Password = Secret Access Key (encrypted in DB, decrypted by CredentialRepository)

        // Check if there's a session token in AdditionalConfig (for MFA/temporary credentials)
        string? sessionToken = null;
        if (!string.IsNullOrEmpty(credential.AdditionalConfig))
        {
            try
            {
                var config = JsonSerializer.Deserialize<AwsAdditionalConfig>(credential.AdditionalConfig);
                sessionToken = config?.SessionToken;
            }
            catch
            {
                // If parsing fails, ignore and use basic credentials
            }
        }

        // Use SessionAWSCredentials if session token is provided (MFA/temporary credentials)
        // Otherwise use BasicAWSCredentials (permanent credentials)
        if (!string.IsNullOrEmpty(sessionToken))
        {
            return new SessionAWSCredentials(credential.Username, credential.Password, sessionToken);
        }

        return new BasicAWSCredentials(credential.Username, credential.Password);
    }

    /// <summary>
    /// Creates AWS additional config JSON with session token
    /// </summary>
    public static string? CreateAdditionalConfig(string? sessionToken)
    {
        if (string.IsNullOrEmpty(sessionToken))
        {
            return null;
        }

        var config = new AwsAdditionalConfig { SessionToken = sessionToken };
        return JsonSerializer.Serialize(config);
    }

    /// <summary>
    /// Converts region string to AWS RegionEndpoint
    /// </summary>
    /// <param name="region">AWS region name (e.g., "us-east-1", "eu-west-1")</param>
    /// <returns>AWS RegionEndpoint object</returns>
    public static Amazon.RegionEndpoint GetRegionEndpoint(string region)
    {
        return Amazon.RegionEndpoint.GetBySystemName(region);
    }
}

/// <summary>
/// Additional configuration for AWS credentials (stored encrypted in AdditionalConfig field)
/// </summary>
public class AwsAdditionalConfig
{
    public string? SessionToken { get; set; }
}

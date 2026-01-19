using Amazon.Runtime;
using Nanuq.Common.Records;

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
        return new BasicAWSCredentials(credential.Username, credential.Password);
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

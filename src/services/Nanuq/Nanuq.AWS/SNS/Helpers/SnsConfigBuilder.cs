using Amazon.SimpleNotificationService;
using Nanuq.AWS.Common;
using Nanuq.Common.Records;

namespace Nanuq.AWS.SNS.Helpers;

/// <summary>
/// Helper class for building SNS client configurations
/// </summary>
public static class SnsConfigBuilder
{
    /// <summary>
    /// Builds an Amazon SNS client with the specified region and credentials
    /// </summary>
    /// <param name="region">AWS region</param>
    /// <param name="credential">Server credentials containing Access Key and Secret Key</param>
    /// <returns>Configured AmazonSimpleNotificationServiceClient</returns>
    public static AmazonSimpleNotificationServiceClient BuildClient(string region, ServerCredential credential)
    {
        var credentials = AwsCredentialHelper.BuildCredentials(credential);
        var regionEndpoint = AwsCredentialHelper.GetRegionEndpoint(region);

        return new AmazonSimpleNotificationServiceClient(credentials, regionEndpoint);
    }
}

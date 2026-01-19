using Amazon.SQS;
using Nanuq.AWS.Common;
using Nanuq.Common.Records;

namespace Nanuq.AWS.SQS.Helpers;

/// <summary>
/// Helper class for building SQS client configurations
/// </summary>
public static class SqsConfigBuilder
{
    /// <summary>
    /// Builds an Amazon SQS client with the specified region and credentials
    /// </summary>
    /// <param name="region">AWS region</param>
    /// <param name="credential">Server credentials containing Access Key and Secret Key</param>
    /// <returns>Configured AmazonSQSClient</returns>
    public static AmazonSQSClient BuildClient(string region, ServerCredential credential)
    {
        var credentials = AwsCredentialHelper.BuildCredentials(credential);
        var regionEndpoint = AwsCredentialHelper.GetRegionEndpoint(region);

        return new AmazonSQSClient(credentials, regionEndpoint);
    }
}

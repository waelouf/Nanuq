using Nanuq.Common.Records;

namespace Nanuq.Common.Requests;

/// <summary>
/// Request to add an AWS server configuration
/// </summary>
/// <param name="Region">AWS region (e.g., us-east-1, eu-west-1)</param>
/// <param name="Alias">Friendly alias for the server</param>
/// <param name="Environment">Environment name (default: Development)</param>
/// <param name="ServiceType">AWS service type (default: SQS)</param>
public record AddAwsRecordRequest(string Region, string Alias, string Environment = "Development", string ServiceType = "SQS");

public static partial class Extension
{
    public static AwsRecord ToRecord(this AddAwsRecordRequest request)
    {
        return new AwsRecord(request.Region, request.Alias, request.Environment, request.ServiceType);
    }
}

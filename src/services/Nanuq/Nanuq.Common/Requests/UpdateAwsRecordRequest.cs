namespace Nanuq.Common.Requests;

/// <summary>
/// Request to update an AWS server configuration
/// </summary>
/// <param name="Id">Server ID</param>
/// <param name="Region">AWS region</param>
/// <param name="Alias">Friendly alias</param>
/// <param name="Environment">Environment name</param>
/// <param name="ServiceType">AWS service type</param>
public record UpdateAwsRecordRequest(int Id, string Region, string Alias, string Environment, string ServiceType);

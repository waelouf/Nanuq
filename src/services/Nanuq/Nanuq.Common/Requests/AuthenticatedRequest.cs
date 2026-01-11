namespace Nanuq.Common.Requests;

/// <summary>
/// Request containing credential ID for authenticated endpoints
/// </summary>
public record AuthenticatedRequest(int CredentialId);

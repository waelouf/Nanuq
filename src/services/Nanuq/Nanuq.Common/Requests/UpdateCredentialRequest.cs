namespace Nanuq.Common.Requests;

public record UpdateCredentialRequest(
    int Id,
    string? Username,
    string? Password,
    string? AdditionalConfig
);

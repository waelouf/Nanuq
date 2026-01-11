namespace Nanuq.Common.Requests;

public record AddCredentialRequest(
    int ServerId,
    string ServerType,
    string? Username,
    string? Password,
    string? AdditionalConfig
);

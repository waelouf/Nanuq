using Nanuq.Common.Enums;
using Nanuq.Common.Records;

namespace Nanuq.Common.Interfaces;

public interface ICredentialRepository
{
    /// <summary>
    /// Gets credential by server ID and type
    /// </summary>
    Task<ServerCredential?> GetByServerAsync(int serverId, ServerType serverType);

    /// <summary>
    /// Gets credential by ID
    /// </summary>
    Task<ServerCredential?> GetByIdAsync(int id);

    /// <summary>
    /// Adds a new credential (encrypts sensitive data)
    /// </summary>
    Task<int> AddAsync(ServerCredential credential);

    /// <summary>
    /// Updates an existing credential (encrypts sensitive data)
    /// </summary>
    Task<bool> UpdateAsync(ServerCredential credential);

    /// <summary>
    /// Deletes a credential by ID
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Updates the LastUsedAt timestamp for a credential
    /// </summary>
    Task<bool> UpdateLastUsedAsync(int id);
}

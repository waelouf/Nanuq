using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nanuq.Common.Enums;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.EF;
using Nanuq.Security.Interfaces;

namespace Nanuq.Sqlite.Repositories;

public class CredentialRepository : ICredentialRepository, IDisposable
{
    private readonly ILogger<CredentialRepository> logger;
    private readonly NanuqContext dbContext;
    private readonly ICredentialService credentialService;
    private bool disposedValue;

    public CredentialRepository(
        ILogger<CredentialRepository> logger,
        NanuqContext dbContext,
        ICredentialService credentialService)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.credentialService = credentialService;
    }

    public async Task<ServerCredential?> GetByServerAsync(int serverId, ServerType serverType)
    {
        try
        {
            var credential = await dbContext.ServerCredentials
                .FirstOrDefaultAsync(c => c.ServerId == serverId && c.ServerType == serverType.ToString());

            if (credential != null)
            {
                // Decrypt sensitive fields
                credential.Username = DecryptIfNotNull(credential.Username);
                credential.Password = DecryptIfNotNull(credential.Password);
                credential.AdditionalConfig = DecryptIfNotNull(credential.AdditionalConfig);
            }

            return credential;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving credential for server {ServerId} of type {ServerType}", serverId, serverType);
            return null;
        }
    }

    public async Task<ServerCredential?> GetByIdAsync(int id)
    {
        try
        {
            var credential = await dbContext.ServerCredentials.FindAsync(id);

            if (credential != null)
            {
                // Decrypt sensitive fields
                credential.Username = DecryptIfNotNull(credential.Username);
                credential.Password = DecryptIfNotNull(credential.Password);
                credential.AdditionalConfig = DecryptIfNotNull(credential.AdditionalConfig);
            }

            return credential;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving credential by ID {Id}", id);
            return null;
        }
    }

    public async Task<int> AddAsync(ServerCredential credential)
    {
        try
        {
            // Encrypt sensitive fields before saving
            credential.Username = EncryptIfNotNull(credential.Username);
            credential.Password = EncryptIfNotNull(credential.Password);
            credential.AdditionalConfig = EncryptIfNotNull(credential.AdditionalConfig);

            credential.EncryptionKeyId = credentialService.GetEncryptionKeyId();
            credential.CreatedAt = DateTime.UtcNow;
            credential.UpdatedAt = DateTime.UtcNow;

            dbContext.ServerCredentials.Add(credential);
            await dbContext.SaveChangesAsync();

            return credential.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding credential");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(ServerCredential credential)
    {
        try
        {
            var existingCredential = await dbContext.ServerCredentials.FindAsync(credential.Id);
            if (existingCredential == null)
            {
                logger.LogWarning("Credential with ID {Id} not found for update", credential.Id);
                return false;
            }

            // Update fields
            existingCredential.Username = EncryptIfNotNull(credential.Username);
            existingCredential.Password = EncryptIfNotNull(credential.Password);
            existingCredential.AdditionalConfig = EncryptIfNotNull(credential.AdditionalConfig);
            existingCredential.UpdatedAt = DateTime.UtcNow;

            dbContext.ServerCredentials.Update(existingCredential);
            var affectedRows = await dbContext.SaveChangesAsync();

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating credential with ID {Id}", credential.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var credential = await dbContext.ServerCredentials.FindAsync(id);
            if (credential != null)
            {
                dbContext.ServerCredentials.Remove(credential);
                await dbContext.SaveChangesAsync();
                return true;
            }

            logger.LogWarning("Credential with ID {Id} not found for deletion", id);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting credential with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> UpdateLastUsedAsync(int id)
    {
        try
        {
            var credential = await dbContext.ServerCredentials.FindAsync(id);
            if (credential == null)
            {
                logger.LogWarning("Credential with ID {Id} not found for updating LastUsedAt", id);
                return false;
            }

            credential.LastUsedAt = DateTime.UtcNow;
            dbContext.ServerCredentials.Update(credential);
            var affectedRows = await dbContext.SaveChangesAsync();

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating LastUsedAt for credential with ID {Id}", id);
            return false;
        }
    }

    private string? EncryptIfNotNull(string? value)
    {
        return string.IsNullOrEmpty(value) ? value : credentialService.Encrypt(value);
    }

    private string? DecryptIfNotNull(string? value)
    {
        return string.IsNullOrEmpty(value) ? value : credentialService.Decrypt(value);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                dbContext.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

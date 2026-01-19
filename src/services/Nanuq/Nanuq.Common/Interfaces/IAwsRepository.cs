using Nanuq.Common.Records;

namespace Nanuq.Common.Interfaces;

/// <summary>
/// Interface for AWS server configuration repository
/// </summary>
public interface IAwsRepository
{
    /// <summary>
    /// Adds a new AWS server configuration
    /// </summary>
    Task<int> Add(AwsRecord record);

    /// <summary>
    /// Deletes an AWS server configuration
    /// </summary>
    Task<bool> Delete(int id);

    /// <summary>
    /// Gets a specific AWS server configuration
    /// </summary>
    Task<AwsRecord> Get(int id);

    /// <summary>
    /// Gets all AWS server configurations
    /// </summary>
    Task<IEnumerable<AwsRecord>> GetAll();

    /// <summary>
    /// Updates an existing AWS server configuration
    /// </summary>
    Task<bool> Update(AwsRecord record);
}

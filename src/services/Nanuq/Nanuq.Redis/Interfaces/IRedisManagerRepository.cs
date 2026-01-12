using Nanuq.Common.Records;
using Nanuq.Redis.Entities;

namespace Nanuq.Redis.Interfaces;

public interface IRedisManagerRepository
{
	ServerDetails GetDatabases(string serverUrl, ServerCredential? credential = null);

	DatabaseDetails GetDatabase(string serverUrl, int database, ServerCredential? credential = null);

	Task<bool> SetStringCache(string serverUrl, int database, string key, string value, double? ttlMilliseconds, ServerCredential? credential = null);

	Task<string?> GetStringCache(string serverUrl, int database, string key, ServerCredential? credential = null);

	Task<bool> CheckKeyExists(string serverUrl, int database, string key, ServerCredential? credential = null);

	Task<bool> InvalidateCache(string serverUrl, int database, string key, ServerCredential? credential = null);

	Task<Dictionary<string, string>> GetAllDatabaseStringKeys(string serverUrl, int database, ServerCredential? credential = null);
}

using Nanuq.Redis.Entities;

namespace Nanuq.Redis.Interfaces;

public interface IRedisManagerRepository
{
	ServerDetails GetDatabases(string serverUrl);

	DatabaseDetails GetDatabase(string serverUrl, int database);

	Task<bool> SetStringCache(string serverUrl, int database, string key, string value, double? ttlMilliseconds);

	Task<string?> GetStringCache(string serverUrl, int database, string key);

	Task<bool> CheckKeyExists(string serverUrl, int database, string key);

	Task<bool> InvalidateCache(string serverUrl, int database, string key);

	Task<Dictionary<string, string>> GetAllDatabaseStringKeys(string serverUrl, int database);
}

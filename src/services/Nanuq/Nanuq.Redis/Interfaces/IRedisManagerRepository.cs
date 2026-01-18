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

	// List operations
	Task<long> PushListElementAsync(string serverUrl, int database, string key, string value, bool pushLeft, ServerCredential? credential = null);
	Task<string?> PopListElementAsync(string serverUrl, int database, string key, bool popLeft, ServerCredential? credential = null);
	Task<List<string>> GetListElementsAsync(string serverUrl, int database, string key, ServerCredential? credential = null);
	Task<long> GetListLengthAsync(string serverUrl, int database, string key, ServerCredential? credential = null);
	Task<bool> DeleteListAsync(string serverUrl, int database, string key, ServerCredential? credential = null);
	Task<Dictionary<string, long>> GetAllDatabaseListKeys(string serverUrl, int database, ServerCredential? credential = null);

	// Hash operations
	Task<bool> SetHashFieldAsync(string serverUrl, int database, string key, string field, string value, ServerCredential? credential = null);
	Task<string?> GetHashFieldAsync(string serverUrl, int database, string key, string field, ServerCredential? credential = null);
	Task<Dictionary<string, string>> GetHashAllFieldsAsync(string serverUrl, int database, string key, ServerCredential? credential = null);
	Task<bool> DeleteHashFieldAsync(string serverUrl, int database, string key, string field, ServerCredential? credential = null);
	Task<bool> DeleteHashAsync(string serverUrl, int database, string key, ServerCredential? credential = null);
	Task<Dictionary<string, long>> GetAllDatabaseHashKeys(string serverUrl, int database, ServerCredential? credential = null);

	// Set operations
	Task<bool> AddSetMemberAsync(string serverUrl, int database, string key, string member, ServerCredential? credential = null);
	Task<List<string>> GetSetMembersAsync(string serverUrl, int database, string key, ServerCredential? credential = null);
	Task<bool> RemoveSetMemberAsync(string serverUrl, int database, string key, string member, ServerCredential? credential = null);
	Task<bool> IsSetMemberAsync(string serverUrl, int database, string key, string member, ServerCredential? credential = null);
	Task<long> GetSetCountAsync(string serverUrl, int database, string key, ServerCredential? credential = null);
	Task<bool> DeleteSetAsync(string serverUrl, int database, string key, ServerCredential? credential = null);
	Task<Dictionary<string, long>> GetAllDatabaseSetKeys(string serverUrl, int database, ServerCredential? credential = null);

	// Sorted Set operations
	Task<bool> AddSortedSetMemberAsync(string serverUrl, int database, string key, string member, double score, ServerCredential? credential = null);
	Task<List<(string member, double score)>> GetSortedSetMembersAsync(string serverUrl, int database, string key, bool ascending = true, ServerCredential? credential = null);
	Task<bool> RemoveSortedSetMemberAsync(string serverUrl, int database, string key, string member, ServerCredential? credential = null);
	Task<long> GetSortedSetCountAsync(string serverUrl, int database, string key, ServerCredential? credential = null);
	Task<bool> DeleteSortedSetAsync(string serverUrl, int database, string key, ServerCredential? credential = null);
	Task<Dictionary<string, long>> GetAllDatabaseSortedSetKeys(string serverUrl, int database, ServerCredential? credential = null);

	// Stream operations
	Task<string> AddStreamEntryAsync(string serverUrl, int database, string key, Dictionary<string, string> fields, ServerCredential? credential = null);
	Task<List<Dictionary<string, object>>> GetStreamEntriesAsync(string serverUrl, int database, string key, int count = 100, ServerCredential? credential = null);
	Task<long> GetStreamLengthAsync(string serverUrl, int database, string key, ServerCredential? credential = null);
	Task<bool> DeleteStreamAsync(string serverUrl, int database, string key, ServerCredential? credential = null);
	Task<Dictionary<string, long>> GetAllDatabaseStreamKeys(string serverUrl, int database, ServerCredential? credential = null);
}

using Microsoft.Extensions.Logging;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.Redis.Entities;
using Nanuq.Redis.Helpers;
using Nanuq.Redis.Interfaces;
using StackExchange.Redis;

namespace Nanuq.Redis.Repository;

public class RedisManagerRepository : IRedisManagerRepository
{
	private ILogger<RedisManagerRepository> logger;

	private IAuditLogRepository activityLog;

	public RedisManagerRepository(ILogger<RedisManagerRepository> logger, IAuditLogRepository activityLog)
	{
		this.logger = logger;
		this.activityLog = activityLog;
	}

	public ServerDetails GetDatabases(string serverUrl, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var server = redis.GetServer(serverUrl);
		var info = server.Info();

		var redisDetails = PopulateServerDetails(info);

		redisDetails.DatabaseCount = server.DatabaseCount;

		return redisDetails;
	}

	private ServerDetails PopulateServerDetails(IGrouping<string, KeyValuePair<string, string>>[] info)
	{
		var serverDetails = new ServerDetails();
		foreach (var section in info)
		{
			switch (section.Key)
			{
				case "Server":
					foreach (var pair in section)
					{
						if (string.Equals(pair.Key, "redis_version", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Server.RedisVersion = pair.Value;
						}
						else if (string.Equals(pair.Key, "redis_mode", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Server.RedisMode = pair.Value;
						}
						else if (string.Equals(pair.Key, "os", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Server.OS = pair.Value;
						}
						else if (string.Equals(pair.Key, "tcp_port", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Server.TcpPort = pair.Value;
						}
						else if (string.Equals(pair.Key, "uptime_in_days", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Server.UptimeInDays = pair.Value;
						}
						else if (string.Equals(pair.Key, "executable", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Server.Executable = pair.Value;
						}
					}
					break;

				case "Clients":
					foreach (var pair in section)
					{
						if (string.Equals(pair.Key, "connected_clients", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Clients.ConnectedClients = int.Parse(pair.Value);
						}
						if (string.Equals(pair.Key, "blocked_clients", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Clients.BlockedClients = int.Parse(pair.Value);
						}
					}
					break;

				case "Memory":
					foreach (var pair in section)
					{
						if (string.Equals(pair.Key, "used_memory_human", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Memory.UsedMemoryHuman = pair.Value;
						}
						if (string.Equals(pair.Key, "total_system_memory_human", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Memory.TotalSystemMemoryHuman = pair.Value;
						}
					}
					break;

				case "Stats":
					foreach (var pair in section)
					{
						if (string.Equals(pair.Key, "total_connections_received", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Stats.TotalConnectionsReceived = int.Parse(pair.Value);
						}
						else if (string.Equals(pair.Key, "total_commands_processed", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Stats.TotalCommandsProcessed = int.Parse(pair.Value);
						}
						else if (string.Equals(pair.Key, "total_net_input_bytes", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Stats.TotalNetInputBytes = int.Parse(pair.Value);
						}
						else if (string.Equals(pair.Key, "total_net_output_bytes", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Stats.TotalNetOutputBytes = int.Parse(pair.Value);
						}
						else if (string.Equals(pair.Key, "keyspace_hits", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Stats.KeyspaceHits = int.Parse(pair.Value);
						}
						else if (string.Equals(pair.Key, "keyspace_misses", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Stats.KeyspaceMisses = int.Parse(pair.Value);
						}
						else if (string.Equals(pair.Key, "pubsub_channels", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Stats.PubsubChannels = int.Parse(pair.Value);
						}
						else if (string.Equals(pair.Key, "pubsubshard_channels", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Stats.PubsubshardChannels = int.Parse(pair.Value);
						}
						else if (string.Equals(pair.Key, "total_error_replies", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Stats.TotalErrorReplies = int.Parse(pair.Value);
						}
						else if (string.Equals(pair.Key, "total_reads_processed", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Stats.TotalReadsProcessed = int.Parse(pair.Value);
						}
						else if (string.Equals(pair.Key, "total_writes_processed", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Stats.TotalWritesProcessed = int.Parse(pair.Value);
						}
					}
					break;

				case "Replication":
					foreach (var pair in section)
					{
						if (string.Equals(pair.Key, "role", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Replication.Role = pair.Value;
						}
						else if (string.Equals(pair.Key, "connected_slaves", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Replication.ConnectedSlaves = int.Parse(pair.Value);
						}
						else if (string.Equals(pair.Key, "master_failover_state", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.Replication.MasterFailoverState = pair.Value;
						}
					}
					break;

				case "CPU":
					foreach (var pair in section)
					{
						if (string.Equals(pair.Key, "used_cpu_sys", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.CPU.UsedCpuSys = double.Parse(pair.Value);
						}
						else if (string.Equals(pair.Key, "used_cpu_user", StringComparison.OrdinalIgnoreCase))
						{
							serverDetails.CPU.UsedCpuUser = double.Parse(pair.Value);
						}
					}
					break;

				case "Keyspace":
					foreach (var pair in section)
					{
						var db = new KeyspaceDb();
						db.Database = pair.Key;
						var parts = pair.Value.Split(new char[] { ',' });
						foreach (var part in parts)
						{
							var s = part.Split('=');
							if (s[0] == "keys")
							{
								db.Keys = int.Parse(s[1]);
							}
							else if (s[0] == "expires")
							{
								db.Expires = int.Parse(s[1]);
							}
							else if (s[0] == "avg_ttl")
							{
								db.AvgTtl = int.Parse(s[1]);
							}
						}

						serverDetails.Databases.Add(db);
					}
					break;
			}

		}

		return serverDetails;
	}

	public DatabaseDetails GetDatabase(string serverUrl, int database, ServerCredential? credential = null)
	{
		var databaseDetails = new DatabaseDetails();

		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);
		var server = redis.GetServer(serverUrl);

		var keys = server.Keys(database).ToList();

		foreach (var key in keys)
		{
			var type = db.KeyType(key);

			switch (type)
			{
				case RedisType.String:
					databaseDetails.MessagesCount[Common.Enums.RedisType.String] += 1;
					break;
				case RedisType.List:
					databaseDetails.MessagesCount[Common.Enums.RedisType.List] += db.ListLength(key);
					break;
				case RedisType.Set:
					databaseDetails.MessagesCount[Common.Enums.RedisType.Set] += db.SetLength(key);
					break;
				case RedisType.SortedSet:
					databaseDetails.MessagesCount[Common.Enums.RedisType.SortedSet] += db.SortedSetLength(key);
					break;
				case RedisType.Hash:
					databaseDetails.MessagesCount[Common.Enums.RedisType.Hash] += db.HashLength(key);
					break;
				case RedisType.Stream:
					databaseDetails.MessagesCount[Common.Enums.RedisType.Stream] += db.StreamLength(key);
					break;
			}
		}

		return databaseDetails;
	}

	public async Task<bool> SetStringCache(string serverUrl, int database, string key, string value, double? ttlMilliseconds, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);
		bool added = false;

		if (ttlMilliseconds.HasValue && ttlMilliseconds.Value > 0)
		{
			added = await db.StringSetAsync(key, value, TimeSpan.FromMilliseconds(ttlMilliseconds.Value));
		}
		else
		{
			added = await db.StringSetAsync(key, value);
		}

		return added;
	}

	public async Task<string?> GetStringCache(string serverUrl, int database, string key, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		var value = await db.StringGetAsync(key);

		return string.IsNullOrEmpty(value) ? null : (string?)value;
	}

	public async Task<bool> CheckKeyExists(string serverUrl, int database, string key, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);
		return await db.KeyExistsAsync(key);
	}

	public async Task<bool> InvalidateCache(string serverUrl, int database, string key, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);
		return await db.KeyDeleteAsync(key);
	}

	public async Task<Dictionary<string, string>> GetAllDatabaseStringKeys(string serverUrl, int database, ServerCredential? credential = null)
	{
		var dbKeys = new List<string>();
		var cacheDictionary = new Dictionary<string, string>();
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var server = redis.GetServer(serverUrl);
		var db = redis.GetDatabase(database);

		var keys = server.Keys(database).
			Where(k => db.KeyType(k) == RedisType.String).ToList();

		foreach (var key in keys)
		{
			var value = await db.StringGetAsync(key);
			cacheDictionary.Add(key, value);
		}

		return cacheDictionary;
	}

	// List operations
	public async Task<long> PushListElementAsync(string serverUrl, int database, string key, string value, bool pushLeft, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		long length = pushLeft
			? await db.ListLeftPushAsync(key, value)
			: await db.ListRightPushAsync(key, value);

		return length;
	}

	public async Task<string?> PopListElementAsync(string serverUrl, int database, string key, bool popLeft, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		var value = popLeft
			? await db.ListLeftPopAsync(key)
			: await db.ListRightPopAsync(key);

		return string.IsNullOrEmpty(value) ? null : (string?)value;
	}

	public async Task<List<string>> GetListElementsAsync(string serverUrl, int database, string key, ServerCredential? credential = null, int limit = 100)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		// Limit to prevent fetching massive datasets
		var values = await db.ListRangeAsync(key, 0, limit - 1);

		return values.Select(v => (string?)v).Where(v => v != null).Select(v => v!).ToList();
	}

	public async Task<long> GetListLengthAsync(string serverUrl, int database, string key, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		return await db.ListLengthAsync(key);
	}

	public async Task<bool> DeleteListAsync(string serverUrl, int database, string key, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		return await db.KeyDeleteAsync(key);
	}

	public async Task<Dictionary<string, long>> GetAllDatabaseListKeys(string serverUrl, int database, ServerCredential? credential = null)
	{
		var listDictionary = new Dictionary<string, long>();
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var server = redis.GetServer(serverUrl);
		var db = redis.GetDatabase(database);

		var keys = server.Keys(database)
			.Where(k => db.KeyType(k) == RedisType.List).ToList();

		foreach (var key in keys)
		{
			var length = await db.ListLengthAsync(key);
			listDictionary.Add(key, length);
		}

		return listDictionary;
	}

	// Hash operations
	public async Task<bool> SetHashFieldAsync(string serverUrl, int database, string key, string field, string value, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		return await db.HashSetAsync(key, field, value);
	}

	public async Task<string?> GetHashFieldAsync(string serverUrl, int database, string key, string field, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		var value = await db.HashGetAsync(key, field);

		return string.IsNullOrEmpty(value) ? null : (string?)value;
	}

	public async Task<Dictionary<string, string>> GetHashAllFieldsAsync(string serverUrl, int database, string key, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		var entries = await db.HashGetAllAsync(key);

		var hashDictionary = new Dictionary<string, string>();
		foreach (var entry in entries)
		{
			hashDictionary.Add(entry.Name, entry.Value);
		}

		return hashDictionary;
	}

	public async Task<bool> DeleteHashFieldAsync(string serverUrl, int database, string key, string field, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		return await db.HashDeleteAsync(key, field);
	}

	public async Task<bool> DeleteHashAsync(string serverUrl, int database, string key, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		return await db.KeyDeleteAsync(key);
	}

	public async Task<Dictionary<string, long>> GetAllDatabaseHashKeys(string serverUrl, int database, ServerCredential? credential = null)
	{
		var hashDictionary = new Dictionary<string, long>();
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var server = redis.GetServer(serverUrl);
		var db = redis.GetDatabase(database);

		var keys = server.Keys(database)
			.Where(k => db.KeyType(k) == RedisType.Hash).ToList();

		foreach (var key in keys)
		{
			var fieldCount = await db.HashLengthAsync(key);
			hashDictionary.Add(key, fieldCount);
		}

		return hashDictionary;
	}

	// Set operations
	public async Task<bool> AddSetMemberAsync(string serverUrl, int database, string key, string member, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		return await db.SetAddAsync(key, member);
	}

	public async Task<List<string>> GetSetMembersAsync(string serverUrl, int database, string key, ServerCredential? credential = null, int limit = 100)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		// Use SetScan for pagination support
		var members = new List<string>();
		await foreach (var member in db.SetScanAsync(key, pageSize: limit))
		{
			if (members.Count >= limit) break;
			var memberStr = (string?)member;
			if (memberStr != null)
				members.Add(memberStr);
		}

		return members;
	}

	public async Task<bool> RemoveSetMemberAsync(string serverUrl, int database, string key, string member, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		return await db.SetRemoveAsync(key, member);
	}

	public async Task<bool> IsSetMemberAsync(string serverUrl, int database, string key, string member, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		return await db.SetContainsAsync(key, member);
	}

	public async Task<long> GetSetCountAsync(string serverUrl, int database, string key, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		return await db.SetLengthAsync(key);
	}

	public async Task<bool> DeleteSetAsync(string serverUrl, int database, string key, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		return await db.KeyDeleteAsync(key);
	}

	public async Task<Dictionary<string, long>> GetAllDatabaseSetKeys(string serverUrl, int database, ServerCredential? credential = null)
	{
		var setDictionary = new Dictionary<string, long>();
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var server = redis.GetServer(serverUrl);
		var db = redis.GetDatabase(database);

		var keys = server.Keys(database)
			.Where(k => db.KeyType(k) == RedisType.Set).ToList();

		foreach (var key in keys)
		{
			var memberCount = await db.SetLengthAsync(key);
			setDictionary.Add(key, memberCount);
		}

		return setDictionary;
	}

	// Sorted Set operations
	public async Task<bool> AddSortedSetMemberAsync(string serverUrl, int database, string key, string member, double score, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);
		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var db = redis.GetDatabase(database);
		return await db.SortedSetAddAsync(key, member, score);
	}

	public async Task<List<(string member, double score)>> GetSortedSetMembersAsync(string serverUrl, int database, string key, bool ascending = true, ServerCredential? credential = null, int limit = 100)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);
		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var db = redis.GetDatabase(database);

		// Use SortedSetRangeByRankWithScoresAsync to support pagination
		var members = await db.SortedSetRangeByRankWithScoresAsync(
			key,
			0,
			limit - 1,
			order: ascending ? Order.Ascending : Order.Descending);

		var result = new List<(string member, double score)>();
		foreach (var entry in members)
		{
			result.Add(((string?)entry.Element ?? string.Empty, entry.Score));
		}

		return result;
	}

	public async Task<bool> RemoveSortedSetMemberAsync(string serverUrl, int database, string key, string member, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);
		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var db = redis.GetDatabase(database);
		return await db.SortedSetRemoveAsync(key, member);
	}

	public async Task<long> GetSortedSetCountAsync(string serverUrl, int database, string key, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);
		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var db = redis.GetDatabase(database);
		return await db.SortedSetLengthAsync(key);
	}

	public async Task<bool> DeleteSortedSetAsync(string serverUrl, int database, string key, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);
		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var db = redis.GetDatabase(database);
		return await db.KeyDeleteAsync(key);
	}

	public async Task<Dictionary<string, long>> GetAllDatabaseSortedSetKeys(string serverUrl, int database, ServerCredential? credential = null)
	{
		var sortedSetDictionary = new Dictionary<string, long>();
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var server = redis.GetServer(serverUrl);
		var db = redis.GetDatabase(database);

		var keys = server.Keys(database)
			.Where(k => db.KeyType(k) == RedisType.SortedSet).ToList();

		foreach (var key in keys)
		{
			var memberCount = await db.SortedSetLengthAsync(key);
			sortedSetDictionary.Add(key, memberCount);
		}

		return sortedSetDictionary;
	}

	// Stream operations
	public async Task<string> AddStreamEntryAsync(string serverUrl, int database, string key, Dictionary<string, string> fields, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);
		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var db = redis.GetDatabase(database);

		var nameValueEntries = fields.Select(kvp => new NameValueEntry(kvp.Key, kvp.Value)).ToArray();
		var entryId = await db.StreamAddAsync(key, nameValueEntries);
		return entryId.ToString();
	}

	public async Task<List<Dictionary<string, object>>> GetStreamEntriesAsync(string serverUrl, int database, string key, ServerCredential? credential = null, int limit = 100)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);
		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var db = redis.GetDatabase(database);

		var entries = await db.StreamReadAsync(key, "0-0", limit);

		var result = new List<Dictionary<string, object>>();
		foreach (var entry in entries)
		{
			var entryDict = new Dictionary<string, object>
			{
				{ "id", entry.Id.ToString() }
			};

			var fieldsDict = new Dictionary<string, string>();
			foreach (var field in entry.Values)
			{
				fieldsDict.Add(field.Name!, field.Value!);
			}
			entryDict.Add("fields", fieldsDict);

			result.Add(entryDict);
		}

		return result;
	}

	public async Task<long> GetStreamLengthAsync(string serverUrl, int database, string key, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);
		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var db = redis.GetDatabase(database);
		return await db.StreamLengthAsync(key);
	}

	public async Task<bool> DeleteStreamAsync(string serverUrl, int database, string key, ServerCredential? credential = null)
	{
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);
		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var db = redis.GetDatabase(database);
		return await db.KeyDeleteAsync(key);
	}

	public async Task<Dictionary<string, long>> GetAllDatabaseStreamKeys(string serverUrl, int database, ServerCredential? credential = null)
	{
		var streamDictionary = new Dictionary<string, long>();
		var configOptions = RedisConfigBuilder.BuildConfig(serverUrl, credential);

		using var redis = ConnectionMultiplexer.Connect(configOptions);
		var server = redis.GetServer(serverUrl);
		var db = redis.GetDatabase(database);

		var keys = server.Keys(database)
			.Where(k => db.KeyType(k) == RedisType.Stream).ToList();

		foreach (var key in keys)
		{
			var entryCount = await db.StreamLengthAsync(key);
			streamDictionary.Add(key, entryCount);
		}

		return streamDictionary;
	}
}

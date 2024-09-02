using Microsoft.Extensions.Logging;
using Nanuq.Common.Interfaces;
using Nanuq.Redis.Entities;
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

	public ServerDetails GetDatabases(string serverUrl)
	{
		var configOptions = new ConfigurationOptions
		{
			EndPoints = { serverUrl },
			AllowAdmin = true
		};

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

	public DatabaseDetails GetDatabase(string serverUrl, int database)
	{
		var databaseDetails = new DatabaseDetails();

		var configOptions = new ConfigurationOptions
		{
			EndPoints = { serverUrl },
			AllowAdmin = true
		};

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

	public async Task<bool> SetStringCache(string serverUrl, int database, string key, string value, double? ttlMilliseconds)
	{
		var configOptions = new ConfigurationOptions
		{
			EndPoints = { serverUrl }
		};

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

	public async Task<string?> GetStringCache(string serverUrl, int database, string key)
	{
		var configOptions = new ConfigurationOptions
		{
			EndPoints = { serverUrl }
		};

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);

		var value = await db.StringGetAsync(key);

		return string.IsNullOrEmpty(value) ? null : (string?)value;
	}

	public async Task<bool> CheckKeyExists(string serverUrl, int database, string key)
	{
		var configOptions = new ConfigurationOptions
		{
			EndPoints = { serverUrl }
		};

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);
		return await db.KeyExistsAsync(key);
	}

	public async Task<bool> InvalidateCache(string serverUrl, int database, string key)
	{
		var configOptions = new ConfigurationOptions
		{
			EndPoints = { serverUrl }
		};

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var db = redis.GetDatabase(database);
		return await db.KeyDeleteAsync(key);
	}

	public async Task<Dictionary<string, string>> GetAllDatabaseStringKeys(string serverUrl, int database)
	{
		var dbKeys = new List<string>();
		var cacheDictionary = new Dictionary<string, string>();
		var configOptions = new ConfigurationOptions
		{
			EndPoints = { serverUrl }
		};

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
}

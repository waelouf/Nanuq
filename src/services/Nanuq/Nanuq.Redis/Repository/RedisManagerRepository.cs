using Microsoft.Extensions.Logging;
using Nanuq.Common.Audit;
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
		var redisDetails = new ServerDetails();

		var configOptions = new ConfigurationOptions
		{
			EndPoints = { serverUrl },
			AllowAdmin = true
		};

		using var redis = ConnectionMultiplexer.Connect(configOptions);

		var server = redis.GetServer(serverUrl);
		redisDetails.DatabaseCount = server.DatabaseCount;
		var info = server.Info();

		//foreach (var section in info)
		//{
		//	Console.WriteLine(section.Key);
		//	foreach (var pair in section)
		//	{
		//		Console.WriteLine($"	{pair.Key}: {pair.Value}");
		//	}
		//}

		return redisDetails;
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

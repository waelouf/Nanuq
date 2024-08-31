using Nanuq.Common.Enums;

namespace Nanuq.Redis.Entities;

public class DatabaseDetails
{
    public DatabaseDetails()
    {
		MessagesCount = new Dictionary<RedisType, long>
		{
			{RedisType.Set, 0},
			{RedisType.Stream, 0},
			{RedisType.String, 0},
			{RedisType.SortedSet, 0 },
			{RedisType.List, 0},
		};
	}

    public Dictionary<RedisType, long> MessagesCount { get; set; }
}

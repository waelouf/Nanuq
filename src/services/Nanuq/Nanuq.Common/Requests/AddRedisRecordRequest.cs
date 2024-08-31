using Nanuq.Common.Records;

namespace Nanuq.Common.Requests;

public record AddRedisRecordRequest(string ServerUrl, string Alias);

public static partial class Extension
{
	public static RedisRecord ToRecord(this AddRedisRecordRequest request)
	{
		return new RedisRecord(request.ServerUrl, request.Alias);
	}
}
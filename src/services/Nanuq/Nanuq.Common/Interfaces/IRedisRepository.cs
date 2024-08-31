using Nanuq.Common.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nanuq.Common.Interfaces;

public interface IRedisRepository
{
	Task<int> Add(RedisRecord record);

	Task<bool> Delete(int id);
	
	Task<RedisRecord> Get(int id);

	Task<IEnumerable<RedisRecord>> GetAll();
}

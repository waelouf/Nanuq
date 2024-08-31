using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nanuq.Sqlite.Repositories;

public class RedisRepository : IRedisRepository
{
	private ILogger<RedisRepository> logger;

	private NanuqContext dbContext;

	public RedisRepository(NanuqContext dbContext, ILogger<RedisRepository> logger)
	{
		this.dbContext = dbContext;
		this.logger = logger;
	}

	public async Task<int> Add(RedisRecord record)
	{
		dbContext.Redis.Add(record);
		await dbContext.SaveChangesAsync();
		return record.Id;
	}


	public async Task<bool> Delete(int id)
	{
		var record = await dbContext.Redis.FindAsync(id);
		if (record != null)
		{
			dbContext.Redis.Remove(record);
			dbContext.SaveChanges();
			return true;
		}

		return false;
	}

	public async Task<RedisRecord> Get(int id)
	{
		var record = await dbContext.Redis.FindAsync(id);
		return record;
	}

	public async Task<IEnumerable<RedisRecord>> GetAll()
	{
		return await dbContext.Redis.ToListAsync();
	}
}

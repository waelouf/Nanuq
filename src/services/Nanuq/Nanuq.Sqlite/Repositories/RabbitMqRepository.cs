using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.EF;

namespace Nanuq.Sqlite.Repositories;

public class RabbitMqRepository : IRabbitMqRepository
{
	private ILogger<RabbitMqRepository> logger;
	
	private NanuqContext dbContext;

	public RabbitMqRepository(ILogger<RabbitMqRepository> logger, NanuqContext dbContext)
	{
		this.logger = logger;
		this.dbContext = dbContext;
	}

	public async Task<int> Add(RabbitMQRecord record)
	{
		dbContext.RabbitMQ.Add(record);
		await dbContext.SaveChangesAsync();
		return record.Id;
	}

	public async Task<bool> Delete(int id)
	{
		var record = await dbContext.RabbitMQ.FindAsync(id);
		if (record != null)
		{
			dbContext.RabbitMQ.Remove(record);
			dbContext.SaveChanges();
			return true;
		}

		return false;
	}

	public async Task<RabbitMQRecord> Get(int id)
	{
		var record = await dbContext.RabbitMQ.FindAsync(id);
		return record;
	}

	public async Task<IEnumerable<RabbitMQRecord>> GetAll()
	{
		return await dbContext.RabbitMQ.ToListAsync();
	}
}

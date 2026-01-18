using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.EF;

namespace Nanuq.Common.Repositories;

public class KafkaRepository : IKafkaRepository, IDisposable
{
	private ILogger<KafkaRepository> logger;

	private NanuqContext dbContext;
	private bool disposedValue;

	public KafkaRepository(ILogger<KafkaRepository> logger, NanuqContext dbContext)
	{
		this.logger = logger;
		this.dbContext = dbContext;
	}

	public async Task<int> Add(KafkaRecord record)
	{
		dbContext.Kafka.Add(record);
		await dbContext.SaveChangesAsync();
		return record.Id;
	}

	public async Task<bool> Delete(int id)
	{
		var record = await dbContext.Kafka.FindAsync(id);
		if (record != null)
		{
			dbContext.Kafka.Remove(record);
			dbContext.SaveChanges();
			return true;
		}

		return false;
	}

	public async Task<KafkaRecord> Get(int id)
	{
		var record = await dbContext.Kafka.FindAsync(id);
		return record;
	}

	public async Task<IEnumerable<KafkaRecord>> GetAll()
	{
		return await dbContext.Kafka.ToListAsync();
	}

	public async Task<bool> Update(KafkaRecord record)
	{
		var recordToUpdate = await dbContext.Kafka.FindAsync(record.Id);
		if (recordToUpdate == null) return false;

		recordToUpdate.BootstrapServer = record.BootstrapServer;
		recordToUpdate.Alias = record.Alias;
		recordToUpdate.Environment = record.Environment;

		dbContext.Kafka.Update(recordToUpdate);
		var affectedRows = await dbContext.SaveChangesAsync();
		return affectedRows > 0;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				dbContext.Dispose();
			}

			disposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	~KafkaRepository()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}

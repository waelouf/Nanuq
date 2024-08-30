using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using Nanuq.EF;
using System.Data;

namespace Nanuq.Common.Repositories;

public class KafkaRepository : IKafkaRepository, IDisposable
{
	private ILogger<KafkaRepository> logger;

	private NanuqContext dbContext;
	private bool disposedValue;

	public KafkaRepository(ILogger<KafkaRepository> logger)
	{
		this.logger = logger;
		dbContext = new NanuqContext();
	}

	public async Task<int> Add(KafkaRecord record)
	{
		dbContext.Kafka.Add(record);
		await dbContext.SaveChangesAsync();
		return record.Id;
	}

	public async Task<bool> Delete(int id)
	{
		var record = dbContext.Kafka.Where(x => x.Id == id).FirstOrDefault();
		if (record == null)
			return false;

		dbContext.Kafka.Remove(record);
		dbContext.SaveChanges();
		return true;
	}

	public async Task<KafkaRecord> Get(int id)
	{
		var record = dbContext.Kafka.Where(x => x.Id == id).FirstOrDefault();

		return await Task.FromResult(record);
	}

	public async Task<IEnumerable<KafkaRecord>> GetAll()
	{
		return await dbContext.Kafka.ToListAsync();
	}

	public async Task<bool> Update(KafkaRecord record)
	{
		var recordToUpdate = dbContext.Kafka.Where(x => x.Id == record.Id).FirstOrDefault();
		if (recordToUpdate == null) return false;

		recordToUpdate.BootstrapServer = record.BootstrapServer;
		recordToUpdate.Alias = record.Alias;

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

using Dapper;
using Microsoft.Extensions.Logging;
using Nanuq.Sqlite.Interfaces;
using Nanuq.Sqlite.Records;
using System.Data;

namespace Nanuq.Sqlite.Repositories;

public class KafkaRepository : IKafkaRepository
{
	private IDbContext dbContext;

	private ILogger<KafkaRepository> logger;

	public KafkaRepository(IDbContext context, ILogger<KafkaRepository> logger)
	{
		dbContext = context;
		this.logger = logger;
	}

	public async Task<int> Add(KafkaRecord record)
	{
		var query = """
			INSERT INTO kafka (bootstrap_server, alias)
			VALUES (@bootstrap_server, @alias)
			""";
		using var conn = dbContext.CreateConnection();
		await conn.ExecuteAsync(query, 
			new { bootstrap_server = record.BootstrapServer, alias = record.Alias }, 
			commandType: CommandType.Text);
		query = "SELECT last_insert_rowid()";
		var insertedId = await conn.QueryAsync<int>(query);
		return insertedId.FirstOrDefault();
	}

	public async Task<bool> Delete(int id)
	{
		var query = """
			DELETE FROM kafka
			WHERE id = @id
			""";
		using var conn = dbContext.CreateConnection();
		var affectedRows = await conn.ExecuteAsync(query, new { id });
		return affectedRows > 0;
	}

	public async Task<KafkaRecord> Get(int id)
	{
		var query = """
			SELECT id, alias, bootstrap_server FROM kafka
			where id = @id
			""";
		using var conn = dbContext.CreateConnection();
		var row =  await conn.QueryFirstAsync(query, new { id });
		return KafkaRecordMapper.CreateKafkaRecord(row);
	}

	public async Task<IEnumerable<KafkaRecord>> GetAll()
	{
		var query = "SELECT id, alias, bootstrap_server FROM kafka";
		using var conn = dbContext.CreateConnection();
		var rows = await conn.QueryAsync(query);
		return rows.Select(row => (KafkaRecord)KafkaRecordMapper.CreateKafkaRecord(row));
	}

	public async Task<bool> Update(KafkaRecord record)
	{
		var query = """
			UPDATE kafka
			SET bootstrap_server = @bootstrap_server,
				alias = @alias
			WHERE id = @id
			""";
		using var conn = dbContext.CreateConnection();
		var affectedRows = await conn.ExecuteAsync(query, new { id = record.Id, bootstrap_server = record.BootstrapServer, alias = record.Alias });
		return affectedRows > 0;
	}
}

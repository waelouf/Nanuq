using Dapper;
using Nanuq.Sqlite.Interfaces;
using Nanuq.Sqlite.Records;
using System.Net.Security;

namespace Nanuq.Sqlite.Repositories;

public class KafkaRepository : IKafkaRepository
{
	private IDbContext dbContext;

    public KafkaRepository(IDbContext context)
    {
        dbContext = context;
    }

    public async Task Add(KafkaRecord record)
	{
		var query = """
			INSERT INTO kafka (bootstrap_server, alias)
			VALUES (@bootstrap_server, @alias)
			""";
		using var conn = dbContext.CreateConnection();
		await conn.ExecuteAsync(query, new { record.BootstrapServer, record.Alias });
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
		return await conn.QueryFirstAsync<KafkaRecord>(query, new { id });
	}

	public async Task<IEnumerable<KafkaRecord>> GetAll()
	{
		var query = "SELECT id, alias, bootstrap_server FROM kafka";
		using var conn = dbContext.CreateConnection();
		return await conn.QueryAsync<KafkaRecord>(query);
	}

	public async Task Update(KafkaRecord record)
	{
		var query = """
			UPDATE kafka
			SET bootstrap_server = @bootstrap_server,
				alias = @alias
			""";
		using var conn = dbContext.CreateConnection();
		await conn.ExecuteAsync(query, new { record.BootstrapServer, record.Alias });
	}
}

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Nanuq.Common.Interfaces;
using System.Data;

namespace Nanuq.Sqlite;

public class DbContext : IDbContext
{
	protected readonly IConfiguration Configuration;

	public DbContext(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public IDbConnection CreateConnection()
	{
		return new SqliteConnection(Configuration.GetConnectionString("NanuqSqliteConfigurations"));
	}
}
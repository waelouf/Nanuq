using System.Data;

namespace Nanuq.Sqlite.Interfaces;

public  interface IDbContext
{
	IDbConnection CreateConnection();
}

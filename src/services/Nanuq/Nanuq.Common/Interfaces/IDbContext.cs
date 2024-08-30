using System.Data;

namespace Nanuq.Common.Interfaces;

public  interface IDbContext
{
	IDbConnection CreateConnection();
}

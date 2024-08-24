using Nanuq.Sqlite.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nanuq.Sqlite.Interfaces;

public interface IActivityLogRepository
{
	Task<IEnumerable<ActivityType>> GetAllActivityTypes();
}

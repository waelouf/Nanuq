using Nanuq.Common.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nanuq.Common.Interfaces;

public interface IActivityLogRepository
{
	Task<IEnumerable<ActivityType>> GetAllActivityTypes();
	Task<IEnumerable<ActivityLog>> GetAllActivityLogs();
}

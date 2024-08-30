using Microsoft.Extensions.Logging;
using Nanuq.Common.Records;
using Nanuq.Common.Interfaces;
using Nanuq.EF;

namespace Nanuq.Sqlite.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
	private ILogger<ActivityLogRepository> logger;

	NanuqContext context;

	public ActivityLogRepository(ILogger<ActivityLogRepository> logger)
	{
		this.logger = logger;
		context = new NanuqContext();
	}

	public async Task<IEnumerable<ActivityType>> GetAllActivityTypes()
	{
		try
		{
			var types= context.ActivityTypes.ToList();
			return await Task.FromResult(types);
		}
		catch (Exception ex)
		{
			logger.LogError(ex.Message, ex);
			throw;
		}
	}

	public async Task<IEnumerable<ActivityLog>> GetAllActivityLogs()
	{

		try
		{
			var logs = context.ActivityLogs.ToList();
			return await Task.FromResult(logs);
		}
		catch (Exception ex)
		{
			logger.LogError(ex.Message, ex);
			throw;
		}
	}
}

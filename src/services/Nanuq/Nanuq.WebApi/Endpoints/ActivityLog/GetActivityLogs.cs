using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;
using ActivityLogRecord = Nanuq.Common.Records.ActivityLog;

namespace Nanuq.WebApi.Endpoints.ActivityLog;

public class GetActivityLogs : EndpointWithoutRequest<IEnumerable<ActivityLogRecord>>
{
	private IActivityLogRepository activityLogRepository;

	public GetActivityLogs(IActivityLogRepository activityLogRepository)
	{
		this.activityLogRepository = activityLogRepository;
	}

	public override void Configure()
	{
		Get("/activitylog");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var logs = await activityLogRepository.GetAllActivityLogs();
		var sorted = logs.OrderByDescending(l => l.Timestamp);
		await Send.OkAsync(sorted, ct);
	}
}

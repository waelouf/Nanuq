using FastEndpoints;
using Nanuq.Common.Interfaces;
using Nanuq.Common.Records;

namespace Nanuq.WebApi.Endpoints.ActivityLog;

public class GetActivityTypes : EndpointWithoutRequest<IEnumerable<ActivityType>>
{
	private IActivityLogRepository activityLogRepository;

	public GetActivityTypes(IActivityLogRepository activityLogRepository)
	{
		this.activityLogRepository = activityLogRepository;
	}

	public override void Configure()
	{
		Get("/activitylog/types");
		AllowAnonymous();
		Options(b => b.RequireCors(x => x.AllowAnyOrigin()
			.AllowAnyMethod()
			.AllowAnyHeader()));
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var types = await activityLogRepository.GetAllActivityTypes();
		await Send.OkAsync(types, ct);
	}
}

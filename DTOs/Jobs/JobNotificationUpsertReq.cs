using System;

namespace TradePlatform.Api.DTOs.Jobs;

public class JobNotificationUpsertReq
{
	public Guid user_id { get; set; }

	public Guid job_id { get; set; }

	public int notification_type_id { get; set; } = 1;

	public bool is_view { get; set; } = true;
}

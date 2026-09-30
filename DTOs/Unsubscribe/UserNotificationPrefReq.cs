using System;

namespace TradePlatform.Api.DTOs.Unsubscribe;

public class UserNotificationPrefReq
{
	public string? email { get; set; }

	public Guid user_id { get; set; }

	public string? token { get; set; }
}

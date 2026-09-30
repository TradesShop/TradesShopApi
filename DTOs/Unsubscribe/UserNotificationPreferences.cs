using System;
using TradePlatform.Api.DTOs.Common;

namespace TradePlatform.Api.DTOs.Unsubscribe;

public class UserNotificationPreferences : CommonResponseDto
{
	public int notification_id { get; set; }

	public string email { get; set; }

	public Guid user_id { get; set; }

	public bool receive_important { get; set; }

	public bool receive_promotional { get; set; }
}

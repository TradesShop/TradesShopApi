using System;

namespace TradePlatform.Api.DTOs.Unsubscribe;

public class UserNotificationPrefUpd
{
	public string email { get; set; }

	public Guid? user_id { get; set; }

	public string? Token { get; set; }

	public bool receive_important { get; set; }

	public bool receive_promotional { get; set; }
}

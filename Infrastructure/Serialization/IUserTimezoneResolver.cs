using System;

namespace TradePlatform.Api.Infrastructure.Serialization;

public interface IUserTimezoneResolver
{
	TimeZoneInfo GetUserTimezone();
}

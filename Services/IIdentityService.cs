using System;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Services;

public interface IIdentityService
{
	Guid GetUserId();

	UserType GetUserType();

	Guid GetCurrentUserId();

	(Guid? userId, UserType? userType) TryGetIdentity();

	(Guid userId, UserType userType) GetIdentity();

	string GetIpAddress();

	string GetUserAgent();

	string GetUserEmail();
}

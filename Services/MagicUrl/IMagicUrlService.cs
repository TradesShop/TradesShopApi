using System;
using TradePlatform.Api.Enums;
using TradePlatform.Api.Models.Email;
using TradePlatform.Api.Services.Emails.Models;

namespace TradePlatform.Api.Services.MagicUrl;

public interface IMagicUrlService
{
	EmailModelBase Build(Guid user_id, string email, Guid job_id, bool is_customer, string template);

	EmailModelBase BuildUrl(NotifyEmailDataModel notifyEmail, EntityType anyEntity, EventType eventType, bool is_customer, string template);

	EmailModelBase BuildBillingUrl(BillingNotifyEmailDataModel notifyEmail, EntityType anyEntity, EventType eventType, string template);
}

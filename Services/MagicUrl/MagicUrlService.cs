using System;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TradePlatform.Api.DTOs.TeleMetry;
using TradePlatform.Api.Enums;
using TradePlatform.Api.Models.Email;
using TradePlatform.Api.Models.MagicPayLoad;
using TradePlatform.Api.Services.AESHelper;
using TradePlatform.Api.Services.Emails.Models;
using TradePlatform.Api.Services.users;

namespace TradePlatform.Api.Services.MagicUrl;

public class MagicUrlService : IMagicUrlService
{
	private readonly IAesCryptoService _aes;

	private readonly MagicLoginSettings _magic;

	private readonly IUsersService _usersService;

	public MagicUrlService(IAesCryptoService aes, IOptions<MagicLoginSettings> magicOptions, IIdentityService identity, IUsersService usersService)
	{
		_aes = aes;
		_magic = magicOptions.Value;
		_usersService = usersService;
	}

	public EmailModelBase BuildUrl(NotifyEmailDataModel notifyEmail, EntityType anyEntity, EventType eventType, bool is_customer, string template)
	{
		Guid user_id = notifyEmail.receiver_id;
		Guid job_id = notifyEmail.job_id;
		string email = notifyEmail.receiver_email;
		Guid? entity_id = notifyEmail.job_id;
		int entity_type_id = EntityType.JobPosts.Id;
		string redirectPath = (is_customer ? $"/my-jobs?jobid={job_id}&tab=all" : $"/my-account/jobs?jobid={job_id}");
		if (anyEntity != EntityType.JobPosts)
		{
			EntityType t = anyEntity;
			if (t == EntityType.JobPurchase)
			{
				entity_id = notifyEmail.job_purchase_id;
				entity_type_id = EntityType.JobPurchase.Id;
				redirectPath = (is_customer ? $"/my-jobs?jobid={job_id}&eid={notifyEmail.job_purchase_id}&tab=all&etype=jobpurchase" : $"/my-account/jobs?jobid={job_id}&eid={notifyEmail.job_purchase_id}&tab=purchased&etype=jobpurchase");
			}
			else
			{
				EntityType t2 = anyEntity;
				if (t2 == EntityType.JobReview)
				{
					entity_id = notifyEmail.job_purchase_id;
					entity_type_id = EntityType.JobReview.Id;
					redirectPath = (is_customer ? $"/my-jobs?jobid={job_id}&eid={notifyEmail.job_purchase_id}&tab=all&etype=jobreview" : $"/my-account/jobs?jobid={job_id}&eid={notifyEmail.job_purchase_id}&tab=purchased&etype=jobreview");
				}
				else
				{
					EntityType t3 = anyEntity;
					if (t3 == EntityType.JobDispute)
					{
						entity_id = notifyEmail.job_dispute_id;
						entity_type_id = EntityType.JobDispute.Id;
						redirectPath = (is_customer ? $"/my-jobs?jobid={job_id}&eid={notifyEmail.job_dispute_id}&tab=all&etype=jobdispute" : $"/my-account/jobs?jobid={job_id}&eid={notifyEmail.job_dispute_id}&tab=purchased&etype=jobdispute");
					}
				}
			}
		}
		var event_payload = new
		{
			user_id = notifyEmail.sender_id,
			receiver_id = notifyEmail.receiver_id,
			receiver_email = notifyEmail.receiver_email,
			template = template,
			job_id = notifyEmail.job_id,
			entity_id = entity_id,
			entity_type_id = entity_type_id
		};
		MagicPayload payload = new MagicPayload
		{
			user_id = user_id,
			job_id = job_id,
			entity_id = entity_id,
			entity_type_id = entity_type_id,
			expires_at = DateTime.UtcNow.AddDays(90.0)
		};
		try
		{
			user_event anyevent = new user_event
			{
				user_id = notifyEmail.sender_id,
				entity_type_id = anyEntity.Id,
				entity_id = payload.job_id,
				event_type_id = eventType.Id,
				event_action_id = 10,
				source = "system_mailer",
				payload = System.Text.Json.JsonSerializer.Serialize(event_payload)
			};
			_usersService.user_event_insert_async(anyevent);
		}
		catch (Exception)
		{
		}
		string json = JsonConvert.SerializeObject(payload);
		string encrypted = _aes.Encrypt(json);
		string encoded = Uri.EscapeDataString(encrypted);
		string baseMagicUrl = _magic.BaseUrl + "?lk=" + encoded;
		string frontMagicUrl = _magic.FrontendBaseUrl ?? "";
		string encodedRedirect = Uri.EscapeDataString(redirectPath);
		return new EmailModelBase
		{
			BaseMagicUrl = baseMagicUrl,
			ManageJobUrl = baseMagicUrl + "&redirect=" + encodedRedirect,
			PostJobUrl = baseMagicUrl + "&redirect=/quote",
			AccountUrl = baseMagicUrl + (is_customer ? "&redirect=/my-jobs" : "&redirect=/my-account"),
			SettingsUrl = baseMagicUrl + (is_customer ? "&redirect=/my-jobs" : "&redirect=/my-account/settings"),
			MyJobsUrl = baseMagicUrl + (is_customer ? "&redirect=/my-jobs" : "&redirect=/my-account/jobs"),
			HomeUrl = baseMagicUrl + "&redirect=/",
			UnsubscribeUrl = $"{frontMagicUrl}/unsubscribe/{email}/{user_id}?token={encoded}",
			ConversationUrl = baseMagicUrl + "&redirect=" + encodedRedirect,
			ViewProfileUrl = baseMagicUrl + "&redirect=/trader/" + notifyEmail.slug,
			ContactUsUrl = baseMagicUrl + "&redirect=/contact-us",
			ViewMyReviewUrl = baseMagicUrl + (is_customer ? "&redirect=/my-jobs" : "&redirect=/my-account/reviews/reviewed")
		};
	}

	public EmailModelBase Build(Guid user_id, string email, Guid job_id, bool is_customer, string template)
	{
		MagicPayload playload = new MagicPayload
		{
			user_id = user_id,
			job_id = job_id,
			entity_id = job_id,
			entity_type_id = EntityType.JobPosts.Id,
			expires_at = DateTime.UtcNow.AddDays(60.0)
		};
		var event_payload = new
		{
			user_id = user_id,
			receiver_id = user_id,
			receiver_email = email,
			template = template,
			job_id = job_id,
			entity_id = job_id,
			entity_type_id = EntityType.JobPosts.Id
		};
		try
		{
			user_event anyevent = new user_event
			{
				user_id = user_id,
				entity_type_id = event_payload.entity_type_id,
				entity_id = playload.entity_id,
				event_type_id = EventType.JobPosted.Id,
				event_action_id = 10,
				source = "system_mailer",
				payload = System.Text.Json.JsonSerializer.Serialize(event_payload)
			};
			_usersService.user_event_insert_async(anyevent);
		}
		catch (Exception)
		{
		}
		string json = JsonConvert.SerializeObject(playload);
		string encrypted = _aes.Encrypt(json);
		string encoded = Uri.EscapeDataString(encrypted);
		string baseMagicUrl = _magic.BaseUrl + "?lk=" + encoded;
		string frontMagicUrl = _magic.FrontendBaseUrl ?? "";
		string redirectPath = (is_customer ? $"/my-jobs?jobid={job_id}&tab=all" : $"/my-account/jobs?jobid={job_id}&tab=new");
		string encodedRedirect = Uri.EscapeDataString(redirectPath);
		return new EmailModelBase
		{
			BaseMagicUrl = baseMagicUrl,
			ManageJobUrl = baseMagicUrl + "&redirect=" + encodedRedirect,
			PostJobUrl = baseMagicUrl + "&redirect=/quote",
			AccountUrl = baseMagicUrl + (is_customer ? "&redirect=/my-jobs" : "&redirect=/my-account"),
			SettingsUrl = baseMagicUrl + (is_customer ? "&redirect=/my-jobs" : "&redirect=/my-account/settings"),
			MyJobsUrl = baseMagicUrl + (is_customer ? "&redirect=/my-jobs" : "&redirect=/my-account/jobs"),
			HomeUrl = baseMagicUrl + "&redirect=/",
			UnsubscribeUrl = $"{frontMagicUrl}/unsubscribe/{email}/{user_id}?token={encoded}",
			ContactUsUrl = baseMagicUrl + "&redirect=/contact-us"
		};
	}

	public EmailModelBase BuildBillingUrl(BillingNotifyEmailDataModel notifyEmail, EntityType anyEntity, EventType eventType, string template)
	{
		Guid user_id = notifyEmail.receiver_id;
		Guid? entity_id = notifyEmail.entity_id;
		string email = notifyEmail.receiver_email;
		var event_payload = new
		{
			user_id = notifyEmail.receiver_id,
			receiver_id = notifyEmail.receiver_id,
			receiver_email = notifyEmail.receiver_email,
			template = template,
			entity_id = entity_id,
			entity_type_id = anyEntity.Id
		};
		MagicPayload payload = new MagicPayload
		{
			user_id = user_id,
			entity_id = entity_id,
			entity_type_id = notifyEmail.entity_type_id,
			expires_at = DateTime.UtcNow.AddDays(90.0)
		};
		try
		{
			user_event anyevent = new user_event
			{
				user_id = user_id,
				entity_type_id = anyEntity.Id,
				entity_id = entity_id,
				event_type_id = eventType.Id,
				event_action_id = 10,
				source = "stripe_webhook",
				payload = System.Text.Json.JsonSerializer.Serialize(event_payload)
			};
			_usersService.user_event_insert_async(anyevent);
		}
		catch (Exception)
		{
		}
		string redirectPath = "/my-account";
		if (eventType == EventType.SubscriptionUpdated || eventType == EventType.SubscriptionCancelled)
		{
			string plan_type = notifyEmail.plan_type;
			string text = ((!(plan_type == "membership")) ? "/my-account/subscriptions/addons" : "/my-account/subscriptions/membership");
			redirectPath = text;
		}
		else
		{
			redirectPath = "/my-account";
		}
		string json = JsonConvert.SerializeObject(payload);
		string encrypted = _aes.Encrypt(json);
		string encoded = Uri.EscapeDataString(encrypted);
		string baseMagicUrl = _magic.BaseBillingUrl + "?lk=" + encoded;
		string frontMagicUrl = _magic.FrontendBaseUrl ?? "";
		string encodedRedirect = Uri.EscapeDataString(redirectPath);
		return new EmailModelBase
		{
			BaseMagicUrl = baseMagicUrl,
			AccountUrl = baseMagicUrl + "&redirect=/my-account",
			SubscriptionUrl = baseMagicUrl + "&redirect=" + encodedRedirect,
			PostJobUrl = baseMagicUrl + "&redirect=/quote",
			SettingsUrl = baseMagicUrl + "&redirect=/my-account/settings",
			MyJobsUrl = baseMagicUrl + "&redirect=/my-account/jobs",
			HomeUrl = baseMagicUrl + "&redirect=/",
			UnsubscribeUrl = $"{frontMagicUrl}/unsubscribe/{email}/{user_id}?token={encoded}",
			ViewProfileUrl = baseMagicUrl + "&redirect=/trader/" + notifyEmail.slug,
			ContactUsUrl = baseMagicUrl + "&redirect=/contact-us",
			ViewMyReviewUrl = baseMagicUrl + "&redirect=/my-account/reviews/reviewed"
		};
	}
}

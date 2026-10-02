using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using TradePlatform.Api.DTOs.Common;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.Plans;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Subscriptions;

public class UserSubscriptionService : IUserSubscriptionService
{
	private readonly ILogger<StripeWebhookService> _logger;

	private readonly IBusinessRepository _businessRepo;

	private readonly IPlansRepository _plansRepository;

	private readonly ISubscriptionsRepository _subscriptionsRepository;

	private readonly IIdentityService _identityService;

	private readonly StripeClient _stripeClient;

	private readonly IStripeService _stripeService;

	private readonly IPaymentMethodRepository _paymentMethodsRepository;

	public UserSubscriptionService(ILogger<StripeWebhookService> logger, IBusinessRepository businessRepo, ISubscriptionsRepository subscriptionsRepository, IPaymentMethodRepository paymentMethodsRepository, IPlansRepository plansRepository, IIdentityService identityService, IStripeService stripeService, StripeClient stripeClient)
	{
		_logger = logger;
		_businessRepo = businessRepo;
		_subscriptionsRepository = subscriptionsRepository;
		_paymentMethodsRepository = paymentMethodsRepository;
		_plansRepository = plansRepository;
		_identityService = identityService;
		_stripeService = stripeService;
		_stripeClient = stripeClient;
	}
    public async Task SubscriptionCancelRequestAsync(SubscriptionCancelReqDto model)
	{
        await _subscriptionsRepository.SubscriptionCancelRequestAsync(model);
    }
    public async Task<SubscriptionViewDto> GetActiveSubscriptionForUserAsync(Guid user_id, string plan_type)
	{
		return await _subscriptionsRepository.GetActiveSubscriptionForUserAsync(user_id, plan_type);
	}

	public async Task SyncStripeCustomerAddressAsync(Guid user_id, string stripe_customer_id)
	{
		UserAddress address = await _businessRepo.BusinessPrimaryAddressForUserId(user_id);
		CustomerService customerService = new CustomerService(_stripeClient);
		await customerService.UpdateAsync(stripe_customer_id, new CustomerUpdateOptions
		{
			Address = new AddressOptions
			{
				Line1 = (string.IsNullOrWhiteSpace(address?.address_line1) ? null : address.address_line1),
				Line2 = (string.IsNullOrWhiteSpace(address?.address_line2) ? null : address.address_line2),
				City = (string.IsNullOrWhiteSpace(address?.town) ? null : address.town),
				PostalCode = (string.IsNullOrWhiteSpace(address?.postcode) ? null : address.postcode),
				Country = (string.IsNullOrWhiteSpace(address?.country_iso_code) ? null : address.country_iso_code)
			}
		});
	}

	private SubscriptionSelectResponse BuildSubscriptionResponse(Subscription? subscription, PlanPriceByPriceId? newPlan)
	{
		if (subscription == null)
		{
			return new SubscriptionSelectResponse
			{
				stripe_subscription_id = null,
				ready_for_subscription = true,
				requires_payment_method = false,
				client_secret = null,
				status = "active"
			};
		}
		string uxmessage = "Your membership has been successfully activated.";
		if (newPlan != null)
		{
			uxmessage = $"Your {newPlan.plan_type} subscription [{newPlan.name} (£{newPlan.price})] has been successfully activated.";
		}
		SubscriptionSelectResponse response = new SubscriptionSelectResponse
		{
			stripe_subscription_id = subscription.Id,
			status = subscription.Status,
			ready_for_subscription = true,
			requires_payment_method = false,
			client_secret = null,
			message = uxmessage
		};
		switch (subscription.Status)
		{
		case "active":
			response.ready_for_subscription = true;
			break;
		case "incomplete":
			response.ready_for_subscription = false;
			response.client_secret = subscription.LatestInvoice?.ConfirmationSecret?.ClientSecret;
			break;
		case "past_due":
		case "incomplete_expired":
			response.ready_for_subscription = false;
			response.requires_payment_method = true;
			break;
		}
		return response;
	}

	private async Task CancelNotRecurringSubscriptionAsync(Guid user_id, SubscriptionViewDto current_subs)
	{
		Dictionary<string, string> metadata = new Dictionary<string, string>
		{
			{
				"user_id",
				user_id.ToString()
			},
			{
				"subscription_id",
				current_subs.id.ToString()
			},
			{
				"plan_id",
				current_subs.plan_id.ToString()
			},
			{
				"plan_price_id",
				current_subs.plan_price_id.ToString()
			},
			{ "subscription_type", current_subs.plan_type },
			{
				"credits",
				current_subs.credits_per_period.ToString()
			},
			{ "source_type", "subscription" },
			{
				"updated_by",
				_identityService.GetUserId().ToString()
			}
		};
		SubscriptionEventProcessDto subsevent_dto = new SubscriptionEventProcessDto
		{
			subscription_id = current_subs.id,
			stripe_subscription_id = null,
			user_id = user_id,
			plan_price_id = current_subs.plan_price_id,
			status = "cancelled",
			cancel_at_period_end = false,
			canceled_at = DateTime.UtcNow,
			billing_cycle_anchor = DateTime.UtcNow,
			trial_end = DateTime.UtcNow,
			event_type = "customer.subscription.deleted",
			metadata_json = JsonConvert.SerializeObject(metadata),
			actor = "user",
			source = "api"
		};
		await _subscriptionsRepository.SubscriptionEventProcessUpdateAsync(subsevent_dto);
	}

	public async Task SubmitSubscriptionPendingUpsert(Guid user_id, SubscriptionViewDto activeSubs, PlanPriceByPriceId newPlan, string stripe_schedule_id, DateTime effective_date, string? status = "PENDING")
	{
		subscriptionpending_upsert sp_dto = new subscriptionpending_upsert
		{
			user_id = user_id,
			current_subscription_id = activeSubs.id,
			stripe_subscription_id = activeSubs.stripe_subscription_id,
			current_plan_price_id = activeSubs.plan_price_id,
			new_plan_price_id = newPlan.plan_price_id,
			current_stripe_price_id = activeSubs.stripe_price_id,
			new_stripe_price_id = newPlan.stripe_price_id,
			stripe_schedule_id = stripe_schedule_id,
			effective_date = effective_date,
			status = status,
			created_by = _identityService?.GetUserId(),
			actor = "user",
			source = "api"
		};
		await _subscriptionsRepository.subscriptionpending_upsert_async(sp_dto);
	}

	public async Task SaveSubscriptionCancelAsync(Subscription? stripeSubscription, Dictionary<string, string> metadata, Guid? subscription_id, Event? stripeEvent, string? actor = "user", string? source = "api")
	{
		SubscriptionItem subscriptionItem = stripeSubscription?.Items?.Data?.FirstOrDefault();
		SubscriptionEventProcessDto dto = new SubscriptionEventProcessDto
		{
			subscription_id = subscription_id,
			stripe_subscription_id = stripeSubscription?.Id,
			current_period_start = subscriptionItem?.CurrentPeriodStart,
			current_period_end = subscriptionItem?.CurrentPeriodEnd,
			event_type = (stripeEvent.Type ?? "api.subscription.deleted"),
			stripe_event_id = (stripeEvent.Id ?? null),
			metadata_json = JsonConvert.SerializeObject(metadata),
			actor = actor,
			source = source
		};
		await _subscriptionsRepository.SubscriptionCancelAsync(dto);
	}

    public async Task SubscriptionUpdateFromStripeWebhook(Subscription? subscription,Guid plan_price_id, Guid user_id
		, Dictionary<string, string> metadata, Event? stripeEvent)
    {
        SubscriptionItem subscriptionItem = subscription?.Items?.Data?.FirstOrDefault();
        string activePriceId = subscription.Items?.Data?.FirstOrDefault()?.Price?.Id;
        SubscriptionEventProcessDto dto = new SubscriptionEventProcessDto
        {           
            stripe_subscription_id = subscription?.Id,
            plan_price_id= plan_price_id,
            stripe_price_id = activePriceId,           
            status = (subscription?.Status ?? "active"),
            user_id = user_id,
            trial_start = subscription?.TrialStart,
            trial_end = subscription?.TrialEnd,
            current_period_start = subscriptionItem?.CurrentPeriodStart,
            current_period_end = subscriptionItem?.CurrentPeriodEnd,
            billing_cycle_anchor = subscription?.BillingCycleAnchor,
            cancel_at_period_end = subscription?.CancelAtPeriodEnd,
            metadata_json = JsonConvert.SerializeObject(metadata),
            stripe_event_id = (stripeEvent?.Id ?? null),
            event_type = (stripeEvent?.Type ?? ("stripe_schedule.subscription.stripe")),
            actor = "stripe_schedule",
            source = "stripe"
        };
        await _subscriptionsRepository.SubscriptionUpdateFromStripeWebhook(dto);
    }
    public async Task SaveSubscriptionUpsertAsync(Subscription? subscription, Guid user_id, Guid plan_price_id, Dictionary<string, string> metadata, Guid? subscription_id, Event? stripeEvent, string? action = "created", string? actor = "user", string? source = "api")
	{
		SubscriptionItem subscriptionItem = subscription?.Items?.Data?.FirstOrDefault();
		SubscriptionEventProcessDto dto = new SubscriptionEventProcessDto
		{
			subscription_id = subscription_id,
			stripe_subscription_id = subscription?.Id,
			plan_price_id = plan_price_id,
			status = (subscription?.Status ?? "active"),
			user_id = user_id,
			trial_start = subscription?.TrialStart,
			trial_end = subscription?.TrialEnd,
			current_period_start = subscriptionItem?.CurrentPeriodStart,
			current_period_end = subscriptionItem?.CurrentPeriodEnd,
			billing_cycle_anchor = subscription?.BillingCycleAnchor,
			cancel_at_period_end = subscription?.CancelAtPeriodEnd,
			metadata_json = JsonConvert.SerializeObject(metadata),
			stripe_event_id = (stripeEvent?.Id ?? null),
			event_type = (stripeEvent?.Type ?? (source + ".subscription." + action)),
			actor = actor,
			source = source
		};
		await _subscriptionsRepository.SubscriptionUpsertAsync(dto);
	}

	public Dictionary<string, string> BuildAuditMetadata(Guid subsription_id, Guid user_id, PlanPriceByPriceId plan)
	{
		return new Dictionary<string, string>
		{
			{
				"user_id",
				user_id.ToString()
			},
			{
				"subscription_id",
				subsription_id.ToString()
			},
			{
				"entity_id",
				subsription_id.ToString()
			},
			{
				"plan_id",
				plan.plan_id.ToString()
			},
			{
				"plan_price_id",
				plan.plan_price_id.ToString()
			},
			{ "subscription_type", plan.plan_type },
			{
				"credits",
				plan.credits_per_period.ToString()
			},
			{ "source_type", "subscription" },
			{ "entity_type", "subscription" },
			{ "entity_type_id", "3" },
			{
				"updated_at",
				DateTime.UtcNow.ToString("O")
			}
		};
	}

	private static bool IsTerminalStatus(string status)
	{
		if (status == "canceled" || status == "incomplete_expired")
		{
			return true;
		}
		return false;
	}

	private static bool IsProblemStatus(string status)
	{
		switch (status)
		{
		case "trialing":
		case "past_due":
		case "unpaid":
		case "incomplete":
		case "paused":
			return true;
		default:
			return false;
		}
	}

	public async Task<subscriptionpending_view> subsctionpending_update_status(Guid? pending_id, string status, Guid? updated_by)
	{
		return await _subscriptionsRepository.subscriptionpending_update_async(new subscriptionpending_upd
		{
			pending_id = pending_id,
			status = status,
			updated_by = updated_by
		});
	}

	private async Task CancelSubsPendingIfExistsAsync(Guid user_id, SubscriptionViewDto activeSubs)
	{
		subscriptionpending_view subs_pending = await _subscriptionsRepository.ScheduledSubscriptionsViewAsync(new subscriptionpending_req
		{
			search_mode = "BY_SUBSCRIPTION",
			user_id = user_id,
			current_subscription_id = activeSubs.id
		});
		if (subs_pending == null || !string.Equals(subs_pending.status, "PENDING", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(subs_pending.stripe_schedule_id))
		{
			return;
		}
		SubscriptionScheduleService scheduleService = new SubscriptionScheduleService(_stripeClient);
		try
		{
			SubscriptionSchedule schedule = await scheduleService.GetAsync(subs_pending.stripe_schedule_id);
			if (schedule != null && schedule.Status != "canceled" && schedule.Status != "released")
			{
				await scheduleService.CancelAsync(subs_pending.stripe_schedule_id);
			}
		}
		catch (StripeException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
		{
		}
		await subsctionpending_update_status(subs_pending.pending_id, "CANCELLED", user_id);
	}

	private async Task<scheduled_response> ScheduledInfo(PlanPriceByPriceId newPlan, SubscriptionViewDto activesubs, DateTime effective_date)
	{
		return new scheduled_response
		{
			plan_type = activesubs.plan_type,
			current_plan_name = activesubs.plan_name,
			current_plan_price = activesubs.price,
			new_plan_name = newPlan.name,
			new_plan_price = newPlan.price,
			effective_date = effective_date
		};
	}

	private async Task<(bool handled, SubscriptionSelectResponse? response)> HandleExistingSubscriptionAsync(Guid user_id, PlanPriceByPriceId newPlan, SubscriptionViewDto activeSubs)
	{
		SubscriptionService subscriptionService = new SubscriptionService(_stripeClient);
		SubscriptionScheduleService scheduleService = new SubscriptionScheduleService(_stripeClient);
		PlanPriceByPriceId currentPlan = await _plansRepository.GetPlanPriceByPriceId(activeSubs.plan_price_id);
		if (!activeSubs.is_recurring)
		{
			if (!newPlan.is_recurring)
			{
				return (handled: true, response: BuildSubscriptionResponse(null, newPlan));
			}
			await CancelNotRecurringSubscriptionAsync(user_id, activeSubs);
			return (handled: false, response: null);
		}
		Subscription stripeSubs;
		try
		{
			stripeSubs = await subscriptionService.GetAsync(activeSubs.stripe_subscription_id, new SubscriptionGetOptions
			{
				Expand = new List<string> { "items.data.price", "latest_invoice.payment_intent", "schedule" }
			});
		}
		catch (StripeException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
		{
			return (handled: false, response: null);
		}
		if (IsTerminalStatus(stripeSubs.Status))
		{
			if (newPlan.is_recurring)
			{
				return (handled: false, response: null);
			}
			await CancelSubscriptionImmediately(user_id, activeSubs.plan_price_id, activeSubs.stripe_subscription_id);
			return (handled: false, response: null);
		}
		if (IsProblemStatus(stripeSubs.Status))
		{
			if (stripeSubs.Status == "paused")
			{
				await subscriptionService.UpdateAsync(activeSubs.stripe_subscription_id, new SubscriptionUpdateOptions
				{
					PauseCollection = null,
					Metadata = BuildAuditMetadata(activeSubs.id, user_id, newPlan)
				});
				return (handled: true, response: await UpdateSubscriptionAsync(user_id, newPlan.plan_price_id, newPlan.stripe_price_id, activeSubs));
			}
			return (handled: true, response: BuildSubscriptionResponse(stripeSubs, newPlan));
		}
		if (!newPlan.is_recurring)
		{
			Dictionary<string, string> metadata = BuildAuditMetadata(activeSubs.id, user_id, newPlan);
			SubscriptionItem subsItem = stripeSubs.Items.Data.FirstOrDefault();
			DateTime periodEnd = subsItem.CurrentPeriodEnd;
			if (periodEnd <= DateTime.UtcNow)
			{
				await CancelSubscriptionImmediately(user_id, activeSubs.plan_price_id, activeSubs.stripe_subscription_id);
				Guid new_subscription_id = Guid.NewGuid();
				await SaveSubscriptionUpsertAsync(null, user_id, newPlan.plan_price_id, metadata, new_subscription_id, null);
				return (handled: true, response: BuildSubscriptionResponse(stripeSubs, newPlan));
			}
			Dictionary<string, string> currentmetadata = BuildAuditMetadata(activeSubs.id, user_id, currentPlan);
			await SubmitSubscriptionPendingUpsert(user_id, activeSubs, newPlan, await ScheduleSubscriptionCancellationAsync(stripeSubs, currentmetadata), periodEnd);
			SubscriptionSelectResponse subscriptionSelectResponse = new SubscriptionSelectResponse
			{
				status = "PENDING"
			};
			SubscriptionSelectResponse subscriptionSelectResponse2 = subscriptionSelectResponse;
			subscriptionSelectResponse2.scheduled = await ScheduledInfo(newPlan, activeSubs, periodEnd);
			subscriptionSelectResponse.is_scheduled = true;
			return (handled: true, response: subscriptionSelectResponse);
		}
		if (activeSubs.plan_price_id == newPlan.plan_price_id)
		{
			if (stripeSubs.Schedule != null)
			{
				await scheduleService.ReleaseAsync(stripeSubs.Schedule.Id, new SubscriptionScheduleReleaseOptions
				{
					PreserveCancelDate = false
				});
			}
			if (activeSubs.cancel_at_period_end || stripeSubs.CancelAtPeriodEnd)
			{
				await subscriptionService.UpdateAsync(activeSubs.stripe_subscription_id, new SubscriptionUpdateOptions
				{                    
                    ProrationBehavior = "none",
                    CancelAtPeriodEnd = false,
					Metadata = BuildAuditMetadata(activeSubs.id, user_id, newPlan)
				});
			}
			await CancelSubsPendingIfExistsAsync(user_id, activeSubs);
			return (handled: true, response: BuildSubscriptionResponse(await subscriptionService.GetAsync(activeSubs.stripe_subscription_id), newPlan));
		}
		if (stripeSubs.Schedule != null)
		{
			await scheduleService.ReleaseAsync(stripeSubs.Schedule.Id, new SubscriptionScheduleReleaseOptions
			{
				PreserveCancelDate = false
			});
		}
		if (activeSubs.cancel_at_period_end || stripeSubs.CancelAtPeriodEnd)
		{
			await CancelSubsPendingIfExistsAsync(user_id, activeSubs);
			await subscriptionService.UpdateAsync(activeSubs.stripe_subscription_id, new SubscriptionUpdateOptions
			{
				ProrationBehavior = "none",

                CancelAtPeriodEnd = false,
				Metadata = BuildAuditMetadata(activeSubs.id, user_id, currentPlan)
			});
		}
		return (handled: true, response: await UpdateSubscriptionAsync(user_id, newPlan.plan_price_id, newPlan.stripe_price_id, activeSubs));
	}

	public async Task<SubscriptionSelectResponse> CreateSubscriptionAsync(Guid user_id, Guid plan_id, Guid plan_price_id)
	{
		PlanPriceByPriceId anynewplan = await _plansRepository.GetPlanPriceByPriceId(plan_price_id);
		if (anynewplan == null)
		{
			throw new Exception("Invalid plan price");
		}
		SubscriptionViewDto active_subs = await _subscriptionsRepository.GetActiveSubscriptionForUserAsync(user_id, anynewplan.plan_type);
		SubscriptionService subscriptionService = new SubscriptionService(_stripeClient);
		if (active_subs != null)
		{
			(bool, SubscriptionSelectResponse) existingResult = await HandleExistingSubscriptionAsync(user_id, anynewplan, active_subs);
			if (existingResult.Item1)
			{
				return existingResult.Item2;
			}
		}
		string stripe_customer_id = await _stripeService.ResolveStripeCustomerIdAsync(user_id);
		await SyncStripeCustomerAddressAsync(user_id, stripe_customer_id);
		PaymentMethod_db default_pm = await _paymentMethodsRepository.GetDefaultPaymentMethodAsync(user_id);
		if (default_pm == null)
		{
			throw new Exception("No default payment method");
		}
		Guid new_subscription_id = Guid.NewGuid();
		Dictionary<string, string> metadata = BuildAuditMetadata(new_subscription_id, user_id, anynewplan);
		if (!anynewplan.is_recurring)
		{
			await SaveSubscriptionUpsertAsync(null, user_id, plan_price_id, metadata, new_subscription_id, null);
			return BuildSubscriptionResponse(null, anynewplan);
		}
		SubscriptionCreateOptions options = new SubscriptionCreateOptions
		{
			Customer = stripe_customer_id,
			Items = new List<SubscriptionItemOptions>
			{
				new SubscriptionItemOptions
				{
					Price = anynewplan.stripe_price_id
				}
			},
			AutomaticTax = new SubscriptionAutomaticTaxOptions
			{
				Enabled = true
			},
			DefaultPaymentMethod = default_pm.stripe_payment_method_id,
			PaymentBehavior = "allow_incomplete",
			Metadata = metadata,
			InvoiceSettings = new SubscriptionInvoiceSettingsOptions
			{
				Issuer = new SubscriptionInvoiceSettingsIssuerOptions
				{
					Type = "self"
				}
			},
			Expand = new List<string> { "items.data", "latest_invoice", "latest_invoice.confirmation_secret" }
		};
		RequestOptions requestOptions = new RequestOptions
		{
			IdempotencyKey = $"subscription-{user_id}-{new_subscription_id}"
		};
		Subscription subscription = await subscriptionService.CreateAsync(options, requestOptions);
		await SaveSubscriptionUpsertAsync(subscription, user_id, plan_price_id, metadata, new_subscription_id, null);
		SubscriptionSelectResponse anyresponse = BuildSubscriptionResponse(subscription, anynewplan);
		anyresponse.is_scheduled = false;
		return anyresponse;
	}

	public async Task<SubscriptionSelectResponse> SelectSubscriptionAsync(Guid user_id, Guid plan_id, Guid plan_price_id)
	{
		if (await _plansRepository.GetPlanByIdAsync(plan_id) == null)
		{
			throw new Exception("Invalid plan");
		}
		PlanPrice price = await _plansRepository.GetPlanPriceByIdAsync(plan_price_id);
		if (price == null || price.plan_id != plan_id)
		{
			throw new Exception("Invalid plan price");
		}
		string stripe_customer_id = await _stripeService.ResolveStripeCustomerIdAsync(user_id);
		PaymentMethod_db default_pm_db = await _paymentMethodsRepository.GetDefaultPaymentMethodAsync(user_id);
		PaymentMethod default_pm = null;
		if (default_pm_db != null && !string.IsNullOrWhiteSpace(default_pm_db.stripe_payment_method_id))
		{
			try
			{
				PaymentMethodService pmService = new PaymentMethodService(_stripeClient);
				default_pm = await pmService.GetAsync(default_pm_db.stripe_payment_method_id);
				if (default_pm == null || default_pm.CustomerId != stripe_customer_id || default_pm.Type != "card")
				{
					default_pm = null;
				}
				if (default_pm?.Card != null)
				{
					DateTime now = DateTime.UtcNow;
					if (default_pm.Card.ExpYear < now.Year || (default_pm.Card.ExpYear == now.Year && default_pm.Card.ExpMonth < now.Month))
					{
						default_pm = null;
					}
				}
			}
			catch (StripeException)
			{
				default_pm = null;
			}
		}
		if (default_pm == null)
		{
			return new SubscriptionSelectResponse
			{
				requires_payment_method = true,
				client_secret = null,
				ready_for_subscription = false,
				subscription_id = null,
				status = "requires_payment_method"
			};
		}
		return await CreateSubscriptionAsync(user_id, plan_id, plan_price_id);
	}

	private async Task CancelSubscriptionEndOfPeriodAsync(Guid effective_userid, Guid plan_price_id, string stripe_subscription_id)
	{
		SubscriptionViewDto sub = await _subscriptionsRepository.GetByStripeSubscriptionIdAsync(stripe_subscription_id);
		if (sub == null)
		{
			throw new InvalidOperationException("Subscription not found");
		}
		if (sub.user_id != effective_userid)
		{
			throw new UnauthorizedAccessException();
		}
		PlanPriceByPriceId plan = await _plansRepository.GetPlanPriceByPriceId(sub.plan_price_id);
		if (plan == null)
		{
			throw new InvalidOperationException("Plan not found");
		}
		Dictionary<string, string> metadata = BuildAuditMetadata(sub.id, effective_userid, plan);
		if (!plan.is_recurring)
		{
			await SaveSubscriptionCancelAsync(null, metadata, sub.id, null);
			return;
		}
		SubscriptionService subscriptionService = new SubscriptionService(_stripeClient);
		SubscriptionScheduleService scheduleService = new SubscriptionScheduleService(_stripeClient);
		Subscription currentStripeSub = await subscriptionService.GetAsync(stripe_subscription_id, new SubscriptionGetOptions
		{
			Expand = new List<string> { "schedule" }
		});
		if (currentStripeSub?.Schedule != null)
		{
			await scheduleService.ReleaseAsync(currentStripeSub.Schedule.Id, new SubscriptionScheduleReleaseOptions
			{
				PreserveCancelDate = true
			});
			currentStripeSub = await subscriptionService.GetAsync(stripe_subscription_id);
		}
		Subscription updatedSubscription = ((!(currentStripeSub.Status == "canceled")) 
			? (await subscriptionService.UpdateAsync(stripe_subscription_id, new SubscriptionUpdateOptions
			{
				CancelAtPeriodEnd = true,
				ProrationBehavior = "none",
				Metadata = metadata
			})) : (await subscriptionService.UpdateAsync(stripe_subscription_id, new SubscriptionUpdateOptions
			{
				Metadata = metadata
			})));
		SubscriptionStatus currentStatus = StripeStatusMapper.MapStripeStatus(sub.status);
		SubscriptionStateMachine sm = new SubscriptionStateMachine(currentStatus);
		string targetStatusString = (updatedSubscription.CancelAtPeriodEnd ? "cancel_scheduled" : updatedSubscription.Status);
		SubscriptionStatus newStatus = StripeStatusMapper.MapStripeStatus(targetStatusString);
		if (currentStatus == newStatus || sm.TryTransitionTo(newStatus, out string reason))
		{
			return;
		}
		_logger.LogWarning("Cannot transition subscription from {CurrentStatus} to {NewStatus}: {Reason}", currentStatus, newStatus, reason);
		throw new InvalidOperationException($"Invalid subscription status transition from {currentStatus} to {newStatus}: {reason}");
	}

	public async Task<SubscriptionCancelResponse> CancelSubscriptionEndOfPeriod(Guid effective_userid, Guid plan_price_id, string stripe_subscription_id)
	{
		await CancelSubscriptionEndOfPeriodAsync(effective_userid, plan_price_id, stripe_subscription_id);
		return new SubscriptionCancelResponse
		{
			stripe_subscription_id = stripe_subscription_id,
			status = "cancel_scheduled",
			plan_price_id = plan_price_id
		};
	}

	public async Task<SubscriptionCancelResponse> CancelSubscriptionImmediately(Guid effective_userid, Guid plan_price_id, string stripe_subscription_id)
	{
		SubscriptionViewDto current_sub = await _subscriptionsRepository.GetByStripeSubscriptionIdAsync(stripe_subscription_id);
		if (current_sub == null)
		{
			throw new InvalidOperationException("Subscription not found");
		}
		PlanPriceByPriceId plan = await _plansRepository.GetPlanPriceByPriceId(current_sub.plan_price_id);
		if (plan == null)
		{
			throw new InvalidOperationException("Plan not found");
		}
		Dictionary<string, string> metadata = BuildAuditMetadata(current_sub.id, effective_userid, plan);
		SubscriptionService subscriptionService = new SubscriptionService(_stripeClient);
		SubscriptionScheduleService scheduleService = new SubscriptionScheduleService(_stripeClient);
		Subscription stripeSub = await subscriptionService.GetAsync(stripe_subscription_id, new SubscriptionGetOptions
		{
			Expand = new List<string> { "schedule" }
		});
		if (stripeSub?.Schedule != null)
		{
			await scheduleService.ReleaseAsync(stripeSub.Schedule.Id, new SubscriptionScheduleReleaseOptions
			{
				PreserveCancelDate = false
			});
			stripeSub = await subscriptionService.GetAsync(stripe_subscription_id);
		}
		Subscription updatedSubscription = ((!(stripeSub.Status == "canceled")) ? (await subscriptionService.CancelAsync(stripe_subscription_id, new SubscriptionCancelOptions
		{
			InvoiceNow = false,
			Prorate = false,
		})) : (await subscriptionService.UpdateAsync(stripe_subscription_id, new SubscriptionUpdateOptions
		{
			Metadata = metadata
		})));
		SubscriptionStatus currentStatus = StripeStatusMapper.MapStripeStatus(current_sub.status);
		SubscriptionStateMachine sm = new SubscriptionStateMachine(currentStatus);
		SubscriptionStatus newStatus = StripeStatusMapper.MapStripeStatus(updatedSubscription.Status);
		if (!sm.TryTransitionTo(newStatus, out string reason))
		{
			throw new InvalidOperationException(reason);
		}
		if (!plan.is_recurring)
		{
			await SaveSubscriptionCancelAsync(null, metadata, current_sub.id, null);
		}
		return new SubscriptionCancelResponse
		{
			stripe_subscription_id = stripe_subscription_id,
			status = updatedSubscription.Status,
			plan_price_id = plan_price_id
		};
	}

	public async Task<SubscriptionCancelResponse> CancelScheduledSubscriptionOnly(Guid effective_userid, Guid plan_price_id, string stripe_subscription_id)
	{
		SubscriptionViewDto currentSub = await _subscriptionsRepository.GetByStripeSubscriptionIdAsync(stripe_subscription_id);
		if (currentSub == null)
		{
			throw new InvalidOperationException("Subscription not found");
		}
		SubscriptionService subscriptionService = new SubscriptionService(_stripeClient);
		SubscriptionScheduleService scheduleService = new SubscriptionScheduleService(_stripeClient);
		string scheduleId = (await subscriptionService.GetAsync(stripe_subscription_id, new SubscriptionGetOptions
		{
			Expand = new List<string> { "schedule" }
		}))?.Schedule?.Id;
		if (string.IsNullOrWhiteSpace(scheduleId))
		{
			await CancelSubsPendingIfExistsAsync(effective_userid, currentSub);
			return new SubscriptionCancelResponse
			{
				stripe_subscription_id = stripe_subscription_id,
				status = "already_cancelled",
				plan_price_id = plan_price_id
			};
		}
		await scheduleService.ReleaseAsync(scheduleId, new SubscriptionScheduleReleaseOptions
		{
			PreserveCancelDate = false
		});
		Subscription _subscription = await subscriptionService.GetAsync(stripe_subscription_id);
		Subscription updatedSubscription;
		if (_subscription.CancelAtPeriodEnd)
		{
			updatedSubscription = await subscriptionService.UpdateAsync(_subscription.Id, new SubscriptionUpdateOptions
			{
                ProrationBehavior = "none",
                CancelAtPeriodEnd = false
			});
		}
		else
		{
			updatedSubscription = ((!_subscription.CancelAt.HasValue) 
				? _subscription : (await subscriptionService.UpdateAsync(_subscription.Id, new SubscriptionUpdateOptions
			{
                ProrationBehavior = "none",
                CancelAt = null
			})));
		}
		return new SubscriptionCancelResponse
		{
			stripe_subscription_id = stripe_subscription_id,
			status = updatedSubscription.Status,
			plan_price_id = plan_price_id
		};
	}

	public async Task<SubscriptionCancelResponse> SetSubscriptionAutoRenewal(Guid effective_userid, Guid plan_price_id, string stripe_subscription_id)
	{
		if (string.IsNullOrWhiteSpace(stripe_subscription_id))
		{
			throw new ArgumentException("Invalid Stripe subscription ID");
		}
		SubscriptionViewDto currentSub = await _subscriptionsRepository.GetByStripeSubscriptionIdAsync(stripe_subscription_id);
		if (currentSub == null)
		{
			throw new InvalidOperationException("Subscription not found");
		}
		if (currentSub.user_id != effective_userid)
		{
			throw new UnauthorizedAccessException("You are not authorised to modify this subscription");
		}
		if (currentSub.plan_price_id != plan_price_id)
		{
			throw new InvalidOperationException("The specified plan price does not match the subscription");
		}
		SubscriptionService subscriptionService = new SubscriptionService(_stripeClient);
		Subscription subscription = await subscriptionService.GetAsync(stripe_subscription_id, new SubscriptionGetOptions
		{
			Expand = new List<string> { "schedule" }
		});
		if (subscription.Status == "canceled")
		{
			throw new InvalidOperationException("This subscription has already been cancelled and cannot be reactivated");
		}
		if (subscription.Status != "active" && subscription.Status != "trialing" && subscription.Status != "past_due")
		{
			throw new InvalidOperationException("Automatic renewal cannot be enabled for a subscription with status '" + subscription.Status + "'");
		}
		if (!subscription.CancelAtPeriodEnd && !subscription.CancelAt.HasValue)
		{
			return new SubscriptionCancelResponse
			{
				stripe_subscription_id = subscription.Id,
				status = subscription.Status,
				plan_price_id = plan_price_id
			};
		}
		Subscription updatedSubscription;
		if (subscription.CancelAtPeriodEnd)
		{
			updatedSubscription = await subscriptionService.UpdateAsync(stripe_subscription_id, new SubscriptionUpdateOptions
			{
                ProrationBehavior = "none",
                CancelAtPeriodEnd = false
			});
		}
		else
		{
			updatedSubscription = ((!subscription.CancelAt.HasValue) ? subscription : (await subscriptionService.UpdateAsync(stripe_subscription_id, new SubscriptionUpdateOptions
			{
                ProrationBehavior = "none",
                CancelAt = null
			})));
		}
		return new SubscriptionCancelResponse
		{
			stripe_subscription_id = updatedSubscription.Id,
			status = updatedSubscription.Status,
			plan_price_id = plan_price_id
		};
	}

	public async Task<SubscriptionSelectResponse> UpdateSubscriptionAsync(Guid effective_userid, Guid new_plan_price_id, string new_stripe_price_id, SubscriptionViewDto current_subs)
	{
		if (current_subs == null)
		{
			throw new InvalidOperationException("Subscription not found");
		}
		if (!current_subs.is_recurring)
		{
			throw new InvalidOperationException("UpdateSubscriptionAsync cannot be used for PAYG subscriptions");
		}
		if (string.IsNullOrEmpty(current_subs.stripe_subscription_id))
		{
			throw new InvalidOperationException("Stripe subscription id missing");
		}
		PlanPriceByPriceId newPlan = await _plansRepository.GetPlanPriceByPriceId(new_plan_price_id);
		if (newPlan == null)
		{
			throw new Exception("Plan not found");
		}
		string stripeSubscriptionId = current_subs.stripe_subscription_id;
		SubscriptionService subscriptionService = new SubscriptionService(_stripeClient);
		Subscription stripeSubscription = await subscriptionService.GetAsync(stripeSubscriptionId, new SubscriptionGetOptions
		{
			Expand = new List<string> { "items.data.price", "latest_invoice.payment_intent" }
		});
		if (stripeSubscription.Status == "canceled" || stripeSubscription.Status == "incomplete_expired")
		{
			return await CreateSubscriptionAsync(effective_userid, newPlan.plan_id, new_plan_price_id);
		}
		SubscriptionItem subs_item = stripeSubscription.Items.Data.FirstOrDefault();
		if (subs_item == null)
		{
			throw new InvalidOperationException("Stripe subscription has no items");
		}
	
		DateTime stripePeriodEnd = subs_item.CurrentPeriodEnd;
		Dictionary<string, string> metadata = BuildAuditMetadata(current_subs.id, effective_userid, newPlan);
		await SyncStripeCustomerAddressAsync(effective_userid, current_subs.stripe_customer_id);
		if (stripePeriodEnd > DateTime.UtcNow)
		{
			SubscriptionScheduleService scheduleService = new SubscriptionScheduleService(_stripeClient);
			subscriptionpending_view existingPending = await _subscriptionsRepository.ScheduledSubscriptionsViewAsync(new subscriptionpending_req
			{
				search_mode = "BY_SUBSCRIPTION",
				user_id = effective_userid,
				current_subscription_id = current_subs.id
			});
			string scheduleId = ((stripeSubscription.Schedule == null) ? (await scheduleService.CreateAsync(new SubscriptionScheduleCreateOptions
			{
				FromSubscription = stripeSubscriptionId
			})).Id : stripeSubscription.Schedule.Id);
			DateTime? currentPeriodStart = stripeSubscription.Items.Data.FirstOrDefault()?.CurrentPeriodStart;
			DateTime? currentPeriodEnd = stripeSubscription.Items.Data.FirstOrDefault()?.CurrentPeriodEnd;
			if (!currentPeriodStart.HasValue || !currentPeriodEnd.HasValue)
			{
				throw new InvalidOperationException("Stripe subscription does not contain a valid billing period.");
			}
			try
			{
				SubscriptionSchedule updatedSchedule = await scheduleService.UpdateAsync(scheduleId, new SubscriptionScheduleUpdateOptions
				{
					DefaultSettings = new SubscriptionScheduleDefaultSettingsOptions
					{
						AutomaticTax = new SubscriptionScheduleDefaultSettingsAutomaticTaxOptions
						{
							Enabled = true
						}
						
                    },
					EndBehavior = "release",                    
                    Metadata = metadata,
					Phases = new List<SubscriptionSchedulePhaseOptions>
					{
						new SubscriptionSchedulePhaseOptions
						{
							StartDate = currentPeriodStart.Value,
							EndDate = stripePeriodEnd,
                            ProrationBehavior = "none",
                            Items = new List<SubscriptionSchedulePhaseItemOptions>
							{
								new SubscriptionSchedulePhaseItemOptions
								{
									Price = current_subs.stripe_price_id,
									Quantity = 1L
								}
							}
						},
						new SubscriptionSchedulePhaseOptions
						{
                          StartDate = stripePeriodEnd,
                          ProrationBehavior = "none",
                            Items = new List<SubscriptionSchedulePhaseItemOptions>
							{                                

                                new SubscriptionSchedulePhaseItemOptions
								{
									Price = new_stripe_price_id,
									Quantity = 1L
								}
							}
						}
					}
				});
				string phase2StripePriceId = (updatedSchedule.Phases?.LastOrDefault())?.Items?.FirstOrDefault()?.PriceId ?? new_stripe_price_id;
				if (existingPending == null)
				{
					await SubmitSubscriptionPendingUpsert(effective_userid, current_subs, newPlan, updatedSchedule.Id, stripePeriodEnd);
				}
				else
				{
					await _subscriptionsRepository.subscriptionpending_update_async(new subscriptionpending_upd
					{
						pending_id = existingPending.pending_id,
						new_plan_price_id = new_plan_price_id,
						new_stripe_price_id = phase2StripePriceId,
						stripe_schedule_id = updatedSchedule.Id,
						effective_date = stripePeriodEnd,
						status = "PENDING",
						updated_by = effective_userid
					});
				}
				SubscriptionSelectResponse subscriptionSelectResponse = new SubscriptionSelectResponse
				{
					status = "PENDING"
				};
				SubscriptionSelectResponse subscriptionSelectResponse2 = subscriptionSelectResponse;
				subscriptionSelectResponse2.scheduled = await ScheduledInfo(newPlan, current_subs, stripePeriodEnd);
				subscriptionSelectResponse.is_scheduled = true;
				return subscriptionSelectResponse;
			}
			catch (StripeException)
			{
				throw;
			}
		}
		SubscriptionItem subscriptionItem = stripeSubscription.Items.Data.FirstOrDefault();
		if (subscriptionItem == null)
		{
			throw new InvalidOperationException("Stripe subscription has no subscription items.");
		}
		SubscriptionUpdateOptions updateOptions = new SubscriptionUpdateOptions
		{
			Items = new List<SubscriptionItemOptions>
			{
				new SubscriptionItemOptions
				{
					Id = subscriptionItem.Id,
					Price = new_stripe_price_id
				}
			},
			AutomaticTax = new SubscriptionAutomaticTaxOptions
			{
				Enabled = true
			},
			ProrationBehavior = "none",
			PaymentBehavior = "allow_incomplete",
			CancelAtPeriodEnd = false,
			Metadata = metadata,
			Expand = new List<string> { "latest_invoice.payment_intent" }
		};
		Subscription updatedSubscription = await subscriptionService.UpdateAsync(stripeSubscriptionId, updateOptions);
		await SaveSubscriptionUpsertAsync(updatedSubscription, effective_userid, new_plan_price_id, metadata, current_subs.id, null, "updated");
		return BuildSubscriptionResponse(updatedSubscription, newPlan);
	}

	public async Task<IEnumerable<SubscriptionsListRes>> SubscriptionsListAsync(CommonSearchReq csreq)
	{
		return await _subscriptionsRepository.SubscriptionsListAsync(csreq);
	}

	public async Task<SubscriptionsHistoryRes> SubscriptionsHistoryAsync(CommonSearchReq csreq)
	{
		return await _subscriptionsRepository.SubscriptionsHistoryAsync(csreq);
	}

	public async Task<string> ScheduleSubscriptionCancellationAsync(Subscription stripeSubscription, Dictionary<string, string>? metadata = null)
	{
		if (stripeSubscription == null)
		{
			throw new ArgumentNullException("stripeSubscription");
		}
		SubscriptionItem item = stripeSubscription.Items.Data.FirstOrDefault();
		if (item == null)
		{
			throw new InvalidOperationException("Subscription has no items.");
		}
		DateTime periodStart = item.CurrentPeriodStart;
		DateTime periodEnd = item.CurrentPeriodEnd;
		SubscriptionScheduleService scheduleService = new SubscriptionScheduleService(_stripeClient);
		string stripe_schedule_id = ((stripeSubscription.Schedule == null) ? (await scheduleService.CreateAsync(new SubscriptionScheduleCreateOptions
		{
			FromSubscription = stripeSubscription.Id
		})).Id : stripeSubscription.Schedule.Id);
		await scheduleService.UpdateAsync(stripe_schedule_id, new SubscriptionScheduleUpdateOptions
		{
            
            EndBehavior = "cancel",
            ProrationBehavior = "none",
            Metadata = metadata,
			Phases = new List<SubscriptionSchedulePhaseOptions>
			{
				new SubscriptionSchedulePhaseOptions
				{
					StartDate = periodStart,
					EndDate = periodEnd,
					Items = new List<SubscriptionSchedulePhaseItemOptions>
					{
						new SubscriptionSchedulePhaseItemOptions
						{
							Price = item.Price.Id,
							Quantity = item.Quantity
						}
					}
				}
			}
		});
		return stripe_schedule_id;
	}

	public async Task<IEnumerable<subscriptionpending_view>> ScheduledSubscriptionsAllAsync(subscriptionpending_req scpreq)
	{
		return await _subscriptionsRepository.ScheduledSubscriptionsAllAsync(scpreq);
	}

	public async Task<subscriptionpending_view> ScheduledSubscriptionsViewAsync(subscriptionpending_req spreq)
	{
		return await _subscriptionsRepository.ScheduledSubscriptionsViewAsync(spreq);
	}
}

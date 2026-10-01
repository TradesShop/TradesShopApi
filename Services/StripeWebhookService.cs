using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;
using TradePlatform.Api.DTOs.Bundles;
using TradePlatform.Api.DTOs.Credits;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.DTOs.Refunds;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Enums;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.Plans;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;
using TradePlatform.Api.Services.BackgroundEmailQueue;
using TradePlatform.Api.Services.Bundles;
using TradePlatform.Api.Services.Credits;
using TradePlatform.Api.Services.Subscriptions;

public class StripeWebhookService : IStripeWebhookService
{
    private readonly ILogger<StripeWebhookService> _logger;
    private readonly IStripeEventsRepository _eventsRepo;
    private readonly ISubscriptionsRepository _subscriptionsRepo;
    private readonly ITdsInvoiceRepository _invoicesRepo;
    private readonly IPaymentsRepository _payments;
    private readonly IPlansRepository _plansRepository;
    private readonly ICreditService _creditService;
    private readonly IBundlePurchaseService _bundlePurchase;
    private readonly IBackgroundEmailQueue _backgroundEmailQueue;
    private readonly IUserSubscriptionService _subscriptionService;
    private readonly InvoiceService _invoiceService;
    private readonly PaymentIntentService _paymentIntentService;
    private readonly IRefundRepository _refundRepo;
    private readonly StripeClient _stripeClient;

    public StripeWebhookService(ILogger<StripeWebhookService> logger, IStripeEventsRepository eventsRepo, ISubscriptionsRepository subscriptionsRepo, ITdsInvoiceRepository invoicesRepo, IPaymentsRepository payments, ICreditService creditService, IBundlePurchaseService bundlePurchase, IPlansRepository plansRepository, IUserSubscriptionService subscriptionService, PaymentIntentService paymentIntentService, IRefundRepository refundRepo, IBackgroundEmailQueue backgroundEmailQueue, StripeClient stripeClient)
    {
        _logger = logger;
        _eventsRepo = eventsRepo;
        _subscriptionsRepo = subscriptionsRepo;
        _invoicesRepo = invoicesRepo;
        _payments = payments;
        _plansRepository = plansRepository;
        _creditService = creditService;
        _bundlePurchase = bundlePurchase;
        _subscriptionService = subscriptionService;
        _refundRepo = refundRepo;
        _paymentIntentService = paymentIntentService;
        _backgroundEmailQueue = backgroundEmailQueue;
        _stripeClient = stripeClient;
    }

    public async Task HandleEventAsync(Event stripeEvent, string rawJson, string? signature)
    {
        _logger.LogInformation("Received Stripe event {EventId} Type={Type}", stripeEvent.Id, stripeEvent.Type);
        StripeEvents existing = await _eventsRepo.GetByStripeEventIdAsync(stripeEvent.Id);
        if (existing != null && existing.processed)
        {
            return;
        }
        StripeEvents log = existing ?? new StripeEvents
        {
            event_id = stripeEvent.Id,
            event_type = stripeEvent.Type,
            api_version = stripeEvent.ApiVersion,
            livemode = stripeEvent.Livemode,
            payload = rawJson,
            signature = signature,
            processed = false,
            received_at = DateTime.UtcNow
        };
        if (existing == null)
        {
            await _eventsRepo.InsertStripeEventAsync(log);
        }
        try
        {
            Subscription stripeSubscription = stripeEvent.Data.Object as Subscription;
            Invoice stripeInvoice = stripeEvent.Data.Object as Invoice;
            SubscriptionSchedule schedule = stripeEvent.Data.Object as SubscriptionSchedule;
            switch (stripeEvent.Type)
            {
                case "invoice.created":
                    await HandleInvoiceInsertOnCreated(stripeInvoice, stripeEvent);
                    break;
                case "invoice.finalized":
                    await HandleInvoiceUpdateOnFinalized(stripeInvoice, stripeEvent);
                    break;
                case "invoice.paid":
                case "invoice.payment_failed":
                case "invoice.voided":
                case "invoice.marked_uncollectible":
                    await HandleInvoiceUpdateOnEventAsync(stripeInvoice, stripeEvent);
                    break;
                case "charge.refunded":
                case "charge.refund.created":
                case "charge.refund.updated":
                    await HandleStripeRefundEvent(stripeEvent);
                    break;
                case "customer.subscription.created":
                case "customer.subscription.updated":
                case "customer.subscription.pending_update_applied":
                    await HandleSubscriptionUpdateUpsert(stripeSubscription, stripeEvent);
                    break;
                case "customer.subscription.deleted":
                    await HandleSubscriptionCancelUpdate(stripeSubscription, stripeEvent);
                    break;
                case "customer.subscription.pending_update_expired":
                    await HandleSubscriptionPendingExpired(stripeSubscription, stripeEvent);
                    break;
                case "subscription_schedule.created":
                case "subscription_schedule.updated":
                case "subscription_schedule.canceled":
                case "subscription_schedule.expiring":
                case "subscription_schedule.released":
                    await HandleSubscriptionScheduleEvent(schedule, stripeEvent);
                    break;
            }
            await _eventsRepo.MarkStripeEventProcessedAsync(log);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error processing Stripe event: " + stripeEvent.Id);
            throw;
        }
    }

    public async Task HandleSubscriptionScheduleEvent(SubscriptionSchedule schedule, Event stripeEvent)
    {
        if (schedule == null)
        {
            _logger.LogWarning("Schedule event received but schedule object missing.");
            return;
        }
        _logger.LogInformation("Processing schedule event: " + stripeEvent.Type + " for schedule " + schedule.Id);
        Dictionary<string, string> metadata = schedule.Metadata;
        if (!metadata.TryGetValue("user_id", out var userIdStr) || !Guid.TryParse(userIdStr, out var _) || !metadata.TryGetValue("subscription_id", out var subIdStr) || !Guid.TryParse(subIdStr, out var _) || !metadata.TryGetValue("plan_price_id", out var priceIdStr) || !Guid.TryParse(priceIdStr, out var _))
        {
            _logger.LogInformation("Schedule metadata: user_id={UserId}, subscription_id={SubscriptionId}, plan_price_id={PlanPriceId}", metadata.GetValueOrDefault("user_id"), metadata.GetValueOrDefault("subscription_id"), metadata.GetValueOrDefault("plan_price_id"));
        }
        subscriptionpending_view subs_pending = await _subscriptionsRepo.ScheduledSubscriptionsViewAsync(new subscriptionpending_req
        {
            search_mode = "BY_SCHEDULE",
            stripe_schedule_id = schedule.Id
        });
        if (subs_pending == null)
        {
            _logger.LogWarning("No pending record found for schedule " + schedule.Id + ". Ignoring.");
        }
        else if (stripeEvent.Type == "subscription_schedule.created")
        {
            await _backgroundEmailQueue.SendScheduleConfirmationEmailAsync(subs_pending, EventType.SubscriptionScheduleCreated);
        }
        else if (stripeEvent.Type == "subscription_schedule.canceled")
        {
            await _subscriptionService.subsctionpending_update_status(subs_pending.pending_id, "CANCELLED", null);
            await _backgroundEmailQueue.SendScheduleCanceledEmailAsync(subs_pending, EventType.SubscriptionScheduleCancelled);
            _logger.LogInformation("Pending schedule " + schedule.Id + " marked CANCELLED.");
        }
        else if (stripeEvent.Type == "subscription_schedule.expiring")
        {
            await _backgroundEmailQueue.SendScheduleExpiringEmailAsync(subs_pending, EventType.SubscriptionScheduleExpiring);
        }
        else if (stripeEvent.Type == "subscription_schedule.released")
        {
            await _subscriptionService.subsctionpending_update_status(subs_pending.pending_id, "RELEASED", null);
            await _backgroundEmailQueue.SendScheduleCanceledEmailAsync(subs_pending, EventType.SubscriptionScheduleCancelled);
            _logger.LogInformation("Pending schedule " + schedule.Id + " marked RELEASED.");
        }
        else if (stripeEvent.Type == "subscription_schedule.updated")
        {
            SubscriptionSchedulePhase futurePhase = schedule.Phases.FirstOrDefault((SubscriptionSchedulePhase p) => p.StartDate >= DateTime.UtcNow);
            if (futurePhase == null)
            {
                return;
            }
            SubscriptionSchedulePhaseItem futureItem = futurePhase.Items.FirstOrDefault();
            if (futureItem != null)
            {
                string oldPriceId = subs_pending.new_stripe_price_id;
                string newPriceId = futureItem.PriceId;
                bool priceChanged = oldPriceId != newPriceId;
                await _subscriptionsRepo.subscriptionpending_update_async(new subscriptionpending_upd
                {
                    pending_id = subs_pending.pending_id,
                    new_stripe_price_id = futureItem.PriceId,
                    updated_by = null
                });
                _logger.LogInformation($"Pending schedule {schedule.Id} updated. New future price: {futureItem.Price}");
                if (priceChanged)
                {
                    await _backgroundEmailQueue.SendScheduleConfirmationEmailAsync(subs_pending, EventType.SubscriptionScheduleUpdated);
                }
            }
        }
        else
        {
            _logger.LogInformation("Schedule event " + stripeEvent.Type + " processed with no state change.");
        }
    }

    private async Task HandleSubscriptionUpdateUpsert(Subscription stripeSubscription, Event stripeEvent)
    {
        string source = "stripe_schedule";
        string actor = "stripe";
        if (stripeSubscription == null)
        {
            _logger.LogWarning("Subscription event received but subscription object missing.");
            return;
        }
        _logger.LogInformation("Processing subscription event {EventType} for subscription {SubscriptionId}", stripeEvent.Type, stripeSubscription.Id);
        // 1. Fetch pre-update record from database
        SubscriptionViewDto existingSubscription = await _subscriptionsRepo.GetByStripeSubscriptionIdAsync(stripeSubscription.Id);
        // 2. Extract metadata with DB fallbacks
        Dictionary<string, string> subs_metadata = stripeSubscription.Metadata ?? new Dictionary<string, string>();
        // Helper to extract GUID from metadata or fallback to database entity
        bool TryGetGuid(string key, Func<SubscriptionViewDto, Guid> fallbackSelector, out Guid result)
        {
            if (subs_metadata != null &&
                subs_metadata.TryGetValue(key, out var val) &&
                Guid.TryParse(val, out result))
            {
                return true;
            }

            if (existingSubscription != null)
            {
                result = fallbackSelector(existingSubscription);
                return true;
            }

            _logger.LogError("Required metadata '{Key}' missing and record not found in DB for subscription {SubscriptionId}", key, stripeSubscription.Id);
            result = Guid.Empty;
            return false;
        }
        // Single clean execution flow
        if (!TryGetGuid("user_id", s => s.user_id, out var user_id) ||
            !TryGetGuid("subscription_id", s => s.id, out var subscription_id) ||
            !TryGetGuid("plan_price_id", s => s.plan_price_id, out var plan_price_id))
        {
            return;
        }
        Guid activePlanPriceId = plan_price_id;
        string currentStripePrice = stripeSubscription.Items?.Data?.FirstOrDefault()?.Price?.Id;
        if (stripeEvent.Type == "customer.subscription.created")
        {
            await _subscriptionService.SaveSubscriptionUpsertAsync(
                stripeSubscription, user_id, activePlanPriceId, subs_metadata, subscription_id, stripeEvent, "updated", actor, source);
            return;
        }
        // await _subscriptionService.SaveSubscriptionUpsertAsync(stripeSubscription, user_id, plan_price_id, subs_metadata, subscription_id, stripeEvent, "updated", actor, source);
        if (stripeEvent.Type == "customer.subscription.updated" || stripeEvent.Type == "customer.subscription.pending_update_applied")
        {
            // Check if pending transition occurred
            subscriptionpending_view scheduledSubs = await _subscriptionsRepo.ScheduledSubscriptionsViewAsync(new subscriptionpending_req
            {
                search_mode = "BY_SUBSCRIPTION",
                user_id = user_id,
                current_subscription_id = subscription_id
            });

            // 3. Check if a pending transition just occurred
            if (scheduledSubs != null && string.Equals(scheduledSubs.status, "PENDING", StringComparison.OrdinalIgnoreCase))
            {
                if (currentStripePrice == scheduledSubs.new_stripe_price_id || stripeEvent.Type == "customer.subscription.pending_update_applied")
                {
                    activePlanPriceId = scheduledSubs.new_plan_price_id;                   
                    await _subscriptionsRepo.subscriptionpending_update_async(new subscriptionpending_upd
                    {
                        current_subscription_id = subscription_id,
                        status = "COMPLETED"
                    });

                    _logger.LogInformation("Completed pending schedule for subscription {SubscriptionId}", subscription_id);
                }
            }
            await _subscriptionService.SubscriptionUpdateFromStripeWebhook(stripeSubscription, activePlanPriceId, user_id, subs_metadata, stripeEvent);
            bool newAutoRenew = !stripeSubscription.CancelAtPeriodEnd;
            bool autoRenewChanged = existingSubscription != null && existingSubscription.auto_renew != newAutoRenew;
            /*This logic is checked and its working fine, no issue found,ok*/
            if (autoRenewChanged)
            {
                existingSubscription.auto_renew = newAutoRenew;
                if (!existingSubscription.auto_renew)
                {
                    await _backgroundEmailQueue.SendSubscriptionAutoRenewalOnEmailAsync(existingSubscription, EventType.SubscriptionAutoNewOn);
                }
                else
                {
                    await _backgroundEmailQueue.SendSubscriptionAutoRenewalOffEmailAsync(existingSubscription, EventType.SubscriptionAutoNewOff);
                }
            }
        }        
    }

    private async Task HandleSubscriptionCancelUpdate(Subscription stripeSubscription, Event stripeEvent)
    {
        string source = "stripe web hook";
        string actor = "stripe";
        if (stripeSubscription == null)
        {
            _logger.LogWarning("Subscription event received but subscription object missing.");
            return;
        }
        _logger.LogInformation("Processing subscription event {EventType} for subscription {SubscriptionId}", stripeEvent.Type, stripeSubscription.Id);
        Dictionary<string, string> subs_metadata = stripeSubscription.Metadata;
        if (!subs_metadata.TryGetValue("subscription_id", out var subIdStr) || !Guid.TryParse(subIdStr, out var subscription_id) ||
        !subs_metadata.TryGetValue("user_id", out var userIdStr) || !Guid.TryParse(userIdStr, out var user_id))
        {
            _logger.LogError("Required metadata missing or invalid for canceled subscription {SubscriptionId}", stripeSubscription.Id);
            return;
        }
        ;
        subscriptionpending_view subs_pending = await _subscriptionsRepo.ScheduledSubscriptionsViewAsync(new subscriptionpending_req
        {
            search_mode = "BY_SUBSCRIPTION",
            user_id = user_id,
            current_subscription_id = subscription_id
        });
        await _subscriptionService.SaveSubscriptionCancelAsync(stripeSubscription, subs_metadata, subscription_id, stripeEvent, source, actor);
        // 5. Send cancellation email safely
        try
        {
            SubscriptionViewDto updatedSubscription = await _subscriptionsRepo.GetByStripeSubscriptionIdAsync(stripeSubscription.Id);
            if (updatedSubscription != null)
            {
                await _backgroundEmailQueue.SendSubsCancellationConfirmationEmail(updatedSubscription);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send cancellation confirmation email for subscription {SubscriptionId}", stripeSubscription.Id);
        }
        // SubscriptionViewDto updatedSubscription = await _subscriptionsRepo.GetByStripeSubscriptionIdAsync(stripeSubscription.Id);
        //await _backgroundEmailQueue.SendSubsCancellationConfirmationEmail(updatedSubscription);
        if (subs_pending != null)
        {
            string pending_status;
            if (!subs_pending.new_is_recurring)
            {
                Guid newSubscriptionId = Guid.NewGuid();
                PlanPriceByPriceId paygPlan = await _plansRepository.GetPlanPriceByPriceId(subs_pending.new_plan_price_id);
                Dictionary<string, string> metadata = _subscriptionService.BuildAuditMetadata(newSubscriptionId, user_id, paygPlan);
                await _subscriptionService.SaveSubscriptionUpsertAsync(null, user_id, subs_pending.new_plan_price_id, metadata, newSubscriptionId, stripeEvent, "created", actor, source);
                pending_status = "COMPLETED";
            }
            else
            {
                pending_status = "CANCELLED";
            }
            await _subscriptionService.subsctionpending_update_status(subs_pending.pending_id, pending_status, null);
        }
    }

    private async Task HandleSubscriptionPendingExpired(Subscription stripeSubscription, Event stripeEvent)
    {
        if (stripeSubscription == null)
        {
            _logger.LogWarning("Subscription event received but subscription object missing.");
            return;
        }
        _logger.LogInformation("Processing subscription event {EventType} for subscription {SubscriptionId}", stripeEvent.Type, stripeSubscription.Id);
        Dictionary<string, string> subs_metadata = stripeSubscription.Metadata;
        Guid subscription_id = Guid.Parse(subs_metadata["subscription_id"]);
        Guid.Parse(subs_metadata["user_id"]);
        await _subscriptionsRepo.subscriptionpending_update_async(new subscriptionpending_upd
        {
            current_subscription_id = subscription_id,
            status = "EXPIRED"
        });
    }

    private async Task HandleInvoiceInsertOnCreated(Invoice stripeInvoice, Event stripeEvent)
    {
        if (stripeInvoice == null)
        {
            return;
        }
        Dictionary<string, string> inv_metadata = stripeInvoice.Metadata;
        if (inv_metadata == null || inv_metadata.Count == 0)
        {
            inv_metadata = stripeInvoice.Lines.Data.FirstOrDefault()?.Metadata;
        }
        if (inv_metadata.ContainsKey("user_id") && inv_metadata.ContainsKey("plan_price_id"))
        {
            if (!inv_metadata.TryGetValue("source_type", out var _))
            {
                _logger.LogWarning("Missing source_type metadata");
                return;
            }
            Guid user_id = Guid.Parse(inv_metadata["user_id"]);
            Guid plan_price_id = Guid.Parse(inv_metadata["plan_price_id"]);
            int entity_type_id = int.Parse(inv_metadata["entity_type_id"]);
            string entity_type = (inv_metadata.ContainsKey("entity_type") ? inv_metadata["entity_type"] : "subscription");
            Guid entity_id = Guid.Parse(inv_metadata["entity_id"]);
            string invoice_type = (inv_metadata.ContainsKey("subscription_type") ? inv_metadata["subscription_type"] : "credit_bundle");
            string metadataJson = JsonConvert.SerializeObject(new
            {
                user_id = user_id,
                plan_price_id = plan_price_id,
                stripe_invoice_id = stripeInvoice.Id,
                status = stripeInvoice.Status,
                event_type = stripeEvent.Type,
                updated_by = "stripe web hook"
            });
            InvoiceUpsertProcessDto invdto = new InvoiceUpsertProcessDto
            {
                stripe_invoice_id = stripeInvoice.Id,
                entity_id = entity_id,
                entity_type_id = entity_type_id,
                entity_type = entity_type,
                invoice_type = invoice_type,
                plan_price_id = plan_price_id,
                user_id = user_id,
                status = stripeInvoice.Status,
                currency = stripeInvoice.Currency,
                stripe_event_id = stripeEvent.Id,
                metadata_json = metadataJson
            };
            await _invoicesRepo.InvoiceInsertOnCreated(invdto);
        }
    }

    private async Task HandleInvoiceUpdateOnFinalized(Invoice stripeInvoice, Event stripeEvent)
    {
        Dictionary<string, string> meta = stripeInvoice.Metadata;
        if (meta == null || meta.Count == 0)
        {
            _ = stripeInvoice.Lines.Data.FirstOrDefault()?.Metadata;
        }
        InvoiceUpsertProcessDto dto = new InvoiceUpsertProcessDto
        {
            stripe_invoice_id = stripeInvoice.Id,
            status = "finalized",
            amount_subtotal = (decimal)stripeInvoice.Subtotal / 100m,
            amount_vat = (decimal)(stripeInvoice.TotalTaxes?.Sum((InvoiceTotalTax x) => x.Amount) ?? 0) / 100m,
            amount_total = (decimal)stripeInvoice.Total / 100m,
            amount_discount = (decimal)(stripeInvoice.TotalDiscountAmounts?.Sum((InvoiceDiscountAmount x) => x.Amount) ?? 0) / 100m,
            invoice_url = stripeInvoice.HostedInvoiceUrl,
            stripe_event_id = stripeEvent.Id,
            stripe_invoice_number = stripeInvoice.Number,
            stripe_receipt_number = stripeInvoice.ReceiptNumber
        };
        await _invoicesRepo.InvoiceUpdateOnFinalized(dto);
    }

    private async Task HandleInvoiceUpdateOnPaid(Invoice stripeInvoice, Event stripeEvent)
    {
        Dictionary<string, string> meta = stripeInvoice.Metadata;
        if (meta == null || meta.Count == 0)
        {
            meta = stripeInvoice.Lines.Data.FirstOrDefault()?.Metadata;
        }
        Guid userId = Guid.Parse(meta["user_id"]);
        Guid planPriceId = Guid.Parse(meta["plan_price_id"]);
        int entityTypeId = int.Parse(meta["entity_type_id"]);
        string entityType = (meta.ContainsKey("entity_type") ? meta["entity_type"] : "subscription");
        Guid entityId = Guid.Parse(meta["entity_id"]);
        string invoice_type = (meta.ContainsKey("subscription_type") ? meta["subscription_type"] : "credit_bundle");
        string chargeId = null;
        InvoiceLineItem line = stripeInvoice.Lines?.Data?.FirstOrDefault();
        DateTime? billingPeriodStart = line?.Period?.Start;
        DateTime? billingPeriodEnd = line?.Period?.End;
        List<InvoiceItemCreateDto> items = stripeInvoice.Lines.Data.Select((InvoiceLineItem invoiceLineItem) => new InvoiceItemCreateDto
        {
            entity_type = entityType,
            entity_id = entityId,
            description = invoiceLineItem.Description,
            quantity = (int)invoiceLineItem.Quantity.Value,
            unit_price = (decimal)invoiceLineItem.Amount / 100m / (decimal)(invoiceLineItem.Quantity ?? 1),
            total_price = (decimal)invoiceLineItem.Amount / 100m
        }).ToList();
        InvoiceService invService = new InvoiceService(_stripeClient);
        string paymentIntentId = (await invService.GetAsync(stripeInvoice.Id, new InvoiceGetOptions
        {
            Expand = new List<string> { "payments.data.payment", "lines.data.tax_amounts" }
        })).Payments.Data.FirstOrDefault()?.Payment?.PaymentIntentId;
        if (!string.IsNullOrWhiteSpace(paymentIntentId))
        {
            PaymentIntentService piService = new PaymentIntentService(_stripeClient);
            chargeId = (await piService.GetAsync(paymentIntentId, new PaymentIntentGetOptions
            {
                Expand = new List<string> { "latest_charge" }
            })).LatestCharge?.Id;
        }
        InvoiceUpsertProcessDto dto = new InvoiceUpsertProcessDto
        {
            stripe_invoice_id = stripeInvoice.Id,
            entity_id = entityId,
            entity_type_id = entityTypeId,
            entity_type = entityType,
            user_id = userId,
            plan_price_id = planPriceId,
            status = "paid",
            amount_subtotal = (decimal)stripeInvoice.Subtotal / 100m,
            amount_vat = (decimal)(stripeInvoice.TotalTaxes?.Sum((InvoiceTotalTax x) => x.Amount) ?? 0) / 100m,
            amount_total = (decimal)stripeInvoice.Total / 100m,
            amount_discount = (decimal)(stripeInvoice.TotalDiscountAmounts?.Sum((InvoiceDiscountAmount x) => x.Amount) ?? 0) / 100m,
            invoice_url = stripeInvoice.HostedInvoiceUrl,
            stripe_payment_intent_id = paymentIntentId,
            stripe_charge_id = chargeId,
            stripe_event_id = stripeEvent.Id,
            stripe_invoice_number = stripeInvoice.Number,
            stripe_receipt_number = stripeInvoice.ReceiptNumber,
            billing_period_start = billingPeriodStart,
            billing_period_end = billingPeriodEnd,
            invoice_type = invoice_type,
            Items = items
        };
        await _invoicesRepo.InvoiceUpdateOnPaid(dto);
    }

    private async Task HandleInvoiceUpdateOnEventAsync(Invoice stripeInvoice, Event stripeEvent)
    {
        if (stripeInvoice == null)
        {
            return;
        }
        Dictionary<string, string> inv_metadata = stripeInvoice.Metadata;
        if (inv_metadata.Count == 0 && stripeInvoice.Lines?.Data != null)
        {
            InvoiceLineItem firstLineItem = stripeInvoice.Lines.Data.FirstOrDefault();
            if (firstLineItem?.Metadata != null)
            {
                inv_metadata = firstLineItem.Metadata;
            }
        }
        if (inv_metadata.ContainsKey("user_id") && inv_metadata.ContainsKey("plan_price_id"))
        {
            switch (stripeEvent.Type)
            {
                case "invoice.payment_succeeded":
                case "invoice.paid":
                    await HandleInvoiceUpdateOnPaid(stripeInvoice, stripeEvent);
                    break;
                case "invoice.payment_failed":
                case "invoice.voided":
                case "invoice.marked_uncollectible":
                    await _invoicesRepo.InvoiceUpdateOnStatusChanged(stripeInvoice.Id, stripeInvoice.Status);
                    break;
            }
        }
    }

    private async Task HandleStripeRefundEvent(Event stripeEvent)
    {
        if (!(stripeEvent.Data.Object is Refund { Metadata: var dictionary } refundObj))
        {
            return;
        }
        if (dictionary == null)
        {
            dictionary = new Dictionary<string, string>();
        }
        Dictionary<string, string> metadata = dictionary;
        if (!metadata.TryGetValue("refund_id", out var refundIdStr) || !Guid.TryParse(refundIdStr, out var refund_id))
        {
            _logger.LogWarning("Stripe refund object missing valid refund_id metadata. RefundId: {StripeRefundId}", refundObj.Id);
            return;
        }
        Guid entity_id = Guid.Empty;
        if (metadata.TryGetValue("entity_id", out var entityIdStr))
        {
            Guid.TryParse(entityIdStr, out entity_id);
        }
        int entity_type_id = 0;
        if (metadata.TryGetValue("entity_type_id", out var entityTypeIdStr))
        {
            int.TryParse(entityTypeIdStr, out entity_type_id);
        }
        int status_id = refundObj.Status switch
        {
            "pending" => 82,
            "failed" => 83,
            "succeeded" => 84,
            _ => 82,
        };
        string comment = stripeEvent.Type switch
        {
            "charge.refund.created" => "Stripe refund created",
            "charge.refund.updated" => "Stripe refund updated: " + refundObj.Status,
            "charge.refunded" => "Stripe refund confirmation received; refund remains successfully completed",
            _ => "Stripe refund event received",
        };
        if (!string.IsNullOrWhiteSpace(refundObj.FailureReason))
        {
            comment = comment + " | Failure reason: " + refundObj.FailureReason;
        }
        string receiptUrl = null;
        if (!string.IsNullOrWhiteSpace(refundObj.ChargeId))
        {
            ChargeService chargeService = new ChargeService(_stripeClient);
            receiptUrl = (await chargeService.GetAsync(refundObj.ChargeId)).ReceiptUrl;
        }
        RefundUpdateRequest updateRequest = new RefundUpdateRequest
        {
            refund_id = refund_id,
            status_id = status_id,
            stripe_refund_id = refundObj.Id,
            stripe_refund_status = refundObj.Status,
            stripe_failure_reason = refundObj.FailureReason,
            receipt_url = receiptUrl,
            comments = comment,
            updated_by = Guid.Empty
        };
        await _refundRepo.UpdateRefundAsync(updateRequest);
        if (status_id == 84)
        {
            await _backgroundEmailQueue.SendRefundConfirmationEmailAsync(refund_id, entity_id, entity_type_id);
        }
    }

    public async Task HandleRefundAsync(string stripe_payment_intent_id, decimal amount, string reason, string reference_type, string reference_id, string user_id)
    {
        Guid userId = Guid.Parse(user_id);
        Guid refId = Guid.Parse(reference_id);
        int credits = (int)amount;
        await RefundCreditsAsync(userId, credits, reference_type, refId, new
        {
            stripe_payment_intent_id = stripe_payment_intent_id,
            reason = reason,
            source = "manual"
        });
    }

    private async Task HandlePaymentIntentFailed(Event stripeEvent)
    {
        PaymentIntent intent = stripeEvent.Data.Object as PaymentIntent;
        Dictionary<string, string> metadata = intent.Metadata;
        BundleCheckoutFailedDto dto = new BundleCheckoutFailedDto
        {
            bundle_order_id = Guid.Parse(metadata["bundle_order_id"])
        };
        await _bundlePurchase.OnBundleOrderMarkFailedAsync(dto);
    }

    private async Task HandleDisputeClosed(Event stripeEvent)
    {
        if (stripeEvent.Data.RawObject is JObject obj)
        {
            string paymentIntentId = obj["payment_intent"]?.ToString();
            if (paymentIntentId != null)
            {
                Guid userId = Guid.Parse(obj["metadata"]?["user_id"].ToString());
                Guid jobId = Guid.Parse(obj["metadata"]?["job_id"].ToString());
                int credits = int.Parse(obj["metadata"]?["credits"].ToString());
                await RefundCreditsAsync(userId, credits, "dispute", jobId, new
                {
                    payment_intent = paymentIntentId,
                    source = "dispute_closed"
                });
            }
        }
    }

    private async Task HandleSubscriptionDeleted(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is Subscription sub)
        {
            await _subscriptionsRepo.MarkCanceledAsync(sub.Id);
        }
    }

    private async Task RefundCreditsAsync(Guid userId, int credits, string referenceType, Guid referenceId, object extraMetadata)
    {
        string metadataJson = JsonConvert.SerializeObject(extraMetadata);
        await _creditService.RefundAsync(new CreditRefundRequest
        {
            user_id = userId,
            credits_to_refund = credits,
            reference_type = referenceType,
            reference_id = referenceId,
            expires_at = DateTime.UtcNow.AddMonths(6),
            metadata = metadataJson
        });
    }

    private async Task HandleSubscriptionUpdateAsync_old(Subscription stripeSubscription, Event stripeEvent)
    {
        string source = "stripe_schedule";
        string actor = "stripe";
        if (stripeSubscription == null)
        {
            _logger.LogWarning("Subscription event received but subscription object missing.");
            return;
        }
        _logger.LogInformation("Processing subscription event {EventType} for subscription {SubscriptionId}", stripeEvent.Type, stripeSubscription.Id);
        Dictionary<string, string> subs_metadata = stripeSubscription.Metadata;
        Guid.Parse(subs_metadata["user_id"]);
        Guid.Parse(subs_metadata["subscription_id"]);
        Guid.Parse(subs_metadata["plan_price_id"]);
        int.Parse(subs_metadata["entity_type_id"]);
        if (subs_metadata.ContainsKey("entity_type"))
        {
            _ = subs_metadata["entity_type"];
        }
        Guid.Parse(subs_metadata["entity_id"]);
        SubscriptionViewDto dbSub = await _subscriptionsRepo.GetByStripeSubscriptionIdAsync(stripeSubscription.Id);
        if (dbSub == null)
        {
            _logger.LogWarning("No local subscription found for Stripe subscription {SubscriptionId}", stripeSubscription.Id);
            return;
        }
        subscriptionpending_view subs_pending = await _subscriptionsRepo.ScheduledSubscriptionsViewAsync(new subscriptionpending_req
        {
            search_mode = "BY_SUBSCRIPTION",
            user_id = dbSub.user_id,
            current_subscription_id = dbSub.id
        });
        if (stripeEvent.Type == "customer.subscription.created")
        {
            await _subscriptionService.SaveSubscriptionUpsertAsync(stripeSubscription, dbSub.user_id, dbSub.plan_price_id, stripeSubscription.Metadata, dbSub.id, stripeEvent, actor, source);
        }
        else if (stripeEvent.Type == "customer.subscription.deleted")
        {
            await _subscriptionService.SaveSubscriptionUpsertAsync(stripeSubscription, dbSub.user_id, dbSub.plan_price_id, stripeSubscription.Metadata, dbSub.id, stripeEvent, actor, source);
            if (subs_pending != null)
            {
                _ = subs_pending.status;
                string pending_status;
                if (!subs_pending.new_is_recurring)
                {
                    await _subscriptionService.SaveSubscriptionUpsertAsync(null, dbSub.user_id, subs_pending.new_plan_price_id, stripeSubscription.Metadata, null, stripeEvent, actor, source);
                    pending_status = "COMPLETED";
                }
                else
                {
                    pending_status = "CANCELLED";
                }
                await _subscriptionService.subsctionpending_update_status(subs_pending.pending_id, pending_status, null);
            }
        }
        else if (stripeEvent.Type == "customer.subscription.pending_update_applied")
        {
            await _subscriptionService.SaveSubscriptionUpsertAsync(stripeSubscription, dbSub.user_id, dbSub.plan_price_id, stripeSubscription.Metadata, dbSub.id, stripeEvent, actor, source);
        }
        else if (stripeEvent.Type == "customer.subscription.pending_update_expired")
        {
            if (subs_pending != null)
            {
                await _subscriptionService.subsctionpending_update_status(subs_pending.pending_id, "EXPIRED", null);
            }
        }
        else if (stripeEvent.Type == "customer.subscription.updated")
        {
            string currentPriceId = stripeSubscription.Items.Data.FirstOrDefault()?.Price?.Id;
            if (subs_pending != null && subs_pending.status == "PENDING" && currentPriceId == subs_pending.new_stripe_price_id)
            {
                _logger.LogInformation("Scheduled subscription change executed for {SubscriptionId}", stripeSubscription.Id);
                await _subscriptionService.SaveSubscriptionUpsertAsync(stripeSubscription, subs_pending.user_id, subs_pending.new_plan_price_id, stripeSubscription.Metadata, subs_pending.current_subscription_id, stripeEvent, "executed", actor, source);
                await _subscriptionService.subsctionpending_update_status(subs_pending.pending_id, "COMPLETED", null);
            }
            else
            {
                await _subscriptionService.SaveSubscriptionUpsertAsync(stripeSubscription, dbSub.user_id, dbSub.plan_price_id, stripeSubscription.Metadata, dbSub.id, stripeEvent, actor, source);
            }
        }
        else
        {
            _logger.LogInformation("Subscription event {EventType} ignored.", stripeEvent.Type);
        }
    }

    private async Task HandleInvoiceEventProcessUpdate_old(Invoice stripeInvoice, Event stripeEvent)
    {
        if (stripeInvoice == null)
        {
            return;
        }
        Dictionary<string, string> inv_metadata = stripeInvoice.Metadata;
        if (inv_metadata == null || inv_metadata.Count == 0)
        {
            inv_metadata = stripeInvoice.Lines.Data.FirstOrDefault()?.Metadata;
        }
        if (!inv_metadata.ContainsKey("user_id") || !inv_metadata.ContainsKey("plan_price_id"))
        {
            return;
        }
        if (!inv_metadata.TryGetValue("source_type", out var sourceType))
        {
            _logger.LogWarning("Missing source_type metadata");
        }
        else if (!(sourceType == "credit_bundle"))
        {
            if (sourceType == "subscription")
            {
                await HandleInvoiceSubscriptionProcessUpdate_old(stripeInvoice, stripeEvent);
            }
            else
            {
                _logger.LogInformation("Ignoring payment_intent for source_type=" + sourceType);
            }
        }
        else
        {
            await HandleInvoiceUpdateOnEventAsync(stripeInvoice, stripeEvent);
        }
    }

    private async Task HandleInvoiceSubscriptionProcessUpdate_old(Invoice stripeInvoice, Event stripeEvent)
    {
        if (stripeInvoice == null)
        {
            return;
        }
        Dictionary<string, string> inv_metadata = stripeInvoice.Metadata;
        if (inv_metadata == null || inv_metadata.Count == 0)
        {
            inv_metadata = stripeInvoice.Lines.Data.FirstOrDefault()?.Metadata;
        }
        InvoiceService invoiceService = new InvoiceService(_stripeClient);
        Invoice anyStripeInvService = await invoiceService.GetAsync(stripeInvoice.Id, new InvoiceGetOptions
        {
            Expand = new List<string> { "payments.data.payment", "customer", "lines.data.tax_amounts", "total_tax_amounts" }
        });
        string payment_intent_id = anyStripeInvService.Payments.Data.FirstOrDefault()?.Payment?.PaymentIntentId;
        _ = anyStripeInvService.AutomaticTax.Enabled;
        Guid user_id = Guid.Parse(inv_metadata["user_id"]);
        Guid plan_price_id = Guid.Parse(inv_metadata["plan_price_id"]);
        PlanPriceByPriceId anyplan = await _plansRepository.GetPlanPriceByPriceId(plan_price_id);
        SubscriptionViewDto subscription = await _subscriptionsRepo.GetActiveSubscriptionForUserAsync(user_id, anyplan.plan_type);
        string invoice_type = (inv_metadata.ContainsKey("subscription_type") ? inv_metadata["subscription_type"] : "membership");
        string billingReason = stripeInvoice.BillingReason;
        _ = billingReason == "subscription_create";
        _ = billingReason == "subscription_cycle";
        _ = billingReason == "subscription_update";
        inv_metadata.ContainsKey("bundle_id");
        inv_metadata.ContainsKey("job_id");
        int entity_type_id = 3;
        Guid entity_id = subscription.id;
        string entity_type = (inv_metadata.ContainsKey("source_type") ? inv_metadata["source_type"] : "subscription");
        if (entity_type == "credit_bundle")
        {
            if (Guid.TryParse(inv_metadata["bundle_order_id"]?.ToString(), out var bundleOrderId))
            {
                entity_id = bundleOrderId;
                entity_type_id = 12;
            }
            else
            {
                entity_id = Guid.Parse("");
            }
        }
        if (invoice_type == null)
        {
            return;
        }
        switch (invoice_type.Length)
        {
            case 10:
                switch (invoice_type[0])
                {
                    case 'm':
                        if (!(invoice_type == "membership"))
                        {
                            return;
                        }
                        break;
                    case 'c':
                        if (!(invoice_type == "categories"))
                        {
                            return;
                        }
                        break;
                    case 'e':
                        if (!(invoice_type == "enterprise"))
                        {
                            return;
                        }
                        break;
                    default:
                        return;
                }
                break;
            case 9:
                if (!(invoice_type == "locations"))
                {
                    return;
                }
                break;
            case 14:
                if (!(invoice_type == "mobile_message"))
                {
                    return;
                }
                break;
            case 7:
                if (!(invoice_type == "premium"))
                {
                    return;
                }
                break;
            case 6:
                if (invoice_type == "bundle")
                {
                }
                return;
            case 3:
                _ = invoice_type == "job";
                return;
            default:
                return;
        }
        string metadataJson = JsonConvert.SerializeObject(new
        {
            user_id = user_id,
            plan_price_id = plan_price_id,
            stripe_invoice_id = stripeInvoice.Id,
            status = stripeInvoice.Status,
            period_start = stripeInvoice.PeriodStart,
            period_end = stripeInvoice.PeriodEnd,
            event_type = stripeEvent.Type,
            invoice_type = invoice_type,
            updated_by = (inv_metadata.ContainsKey("updated_by") ? inv_metadata["updated_by"] : "stripe")
        });
        List<InvoiceItemCreateDto> items = stripeInvoice.Lines.Data.Select((InvoiceLineItem line) => new InvoiceItemCreateDto
        {
            entity_type = entity_type,
            entity_id = entity_id,
            description = line.Description,
            quantity = (int)line.Quantity.Value,
            unit_price = (decimal)line.Amount / 100m / (decimal)(line.Quantity ?? 1),
            total_price = (decimal)line.Amount / 100m
        }).ToList();
        _ = stripeInvoice.StatusTransitions?.PaidAt;
        InvoiceEventProcessDto inv_Event_dto = new InvoiceEventProcessDto
        {
            user_id = user_id,
            plan_price_id = plan_price_id,
            subscription_id = subscription.id,
            stripe_invoice_id = stripeInvoice.Id,
            stripe_payment_intent_id = payment_intent_id,
            status = stripeInvoice.Status,
            invoice_type = invoice_type,
            currency = stripeInvoice.Currency?.ToUpper(),
            subtotal = (decimal)stripeInvoice.Subtotal / 100m,
            tax_amount = (decimal)(stripeInvoice.TotalTaxes?.Sum((InvoiceTotalTax x) => x.Amount) ?? 0) / 100m,
            discount_amount = (decimal)(stripeInvoice.TotalDiscountAmounts?.Sum((InvoiceDiscountAmount x) => x.Amount) ?? 0) / 100m,
            total_amount = (decimal)stripeInvoice.Total / 100m,
            billing_email = stripeInvoice.CustomerEmail,
            billing_period_start = stripeInvoice?.PeriodStart,
            billing_period_end = stripeInvoice?.PeriodEnd,
            issued_at = stripeInvoice.Created,
            paid_at = stripeInvoice.StatusTransitions?.PaidAt,
            due_at = stripeInvoice.DueDate,
            metadata_json = metadataJson,
            stripe_event_id = stripeEvent.Id,
            event_type = stripeEvent.Type,
            invoice_url = stripeInvoice?.InvoicePdf,
            entity_type_id = entity_type_id,
            entity_id = entity_id,
            actor = "stripe_webhook",
            source = "stripe",
            Items = items
        };
        await _invoicesRepo.Invoice_event_process_updateAsync(inv_Event_dto);
    }
}
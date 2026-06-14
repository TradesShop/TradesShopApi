using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Bundles;
using TradePlatform.Api.DTOs.Credits;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Implementations;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services;
using TradePlatform.Api.Services.Bundles;
using TradePlatform.Api.Services.Credits;

public class StripeWebhookService : IStripeWebhookService
{
    private readonly ILogger<StripeWebhookService> _logger;
    private readonly IStripeEventsRepository _eventsRepo;
    private readonly ISubscriptionsRepository _subscriptionsRepo;
    private readonly IInvoicesTshRepository _invoicesRepo;
    private readonly IPaymentsRepository _payments;
    private readonly IPlansRepository _plansRepository;
    private readonly ICreditService _creditService;
    private readonly IBundlePurchaseService _bundlePurchase;

    private readonly SubscriptionService _subscriptionService;
    private readonly Stripe.InvoiceService _invoiceService;
    private readonly PaymentIntentService _paymentIntentService;
    private readonly StripeClient _stripeClient;

    public StripeWebhookService(
         ILogger<StripeWebhookService> logger,
        IStripeEventsRepository eventsRepo,
        ISubscriptionsRepository subscriptionsRepo,
        IInvoicesTshRepository invoicesRepo,
        IPaymentsRepository payments,
        ICreditService creditService,
        IBundlePurchaseService bundlePurchase,
        IPlansRepository plansRepository,
        SubscriptionService subscriptionService,
        Stripe.InvoiceService invoiceService,
        PaymentIntentService paymentIntentService,
        StripeClient stripeClient)
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
        _invoiceService = invoiceService;
        _paymentIntentService = paymentIntentService;
        _stripeClient = stripeClient;
    }

    // =====================================================================
    // 0. MAIN ENTRYPOINT
    // =====================================================================
    public async Task HandleEventAsync(Event stripeEvent, string rawJson, string? signature)
    {
        // 1. IDEMPOTENCY
        var existing = await _eventsRepo.GetByStripeEventIdAsync(stripeEvent.Id);

        if (existing != null && existing.processed)
            return;

        var log = existing ?? new StripeEvents
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
            await _eventsRepo.InsertStripeEventAsync(log);

        try
        {
          
            // Extract subscription if this event contains one
            var stripeSubscription = stripeEvent.Data.Object as Subscription;
            var stripeInvoice = stripeEvent.Data.Object as Invoice;
            
            // 2. ROUTING
            switch (stripeEvent.Type)
            {

                // ------------------------------------------------------------
                // SUBSCRIPTION INVOICE EVENTS
                // ------------------------------------------------------------
                case "invoice.payment_succeeded":
                case "invoice.payment_failed":
                case "invoice.voided":
                case "invoice.marked_uncollectible":
                    await HandleInvoiceEventProcessUpdate(stripeInvoice, stripeEvent);
                    break;
                case "payment_intent.succeeded":
                    await HandlePaymentIntentSucceeded(stripeEvent);
                    break;
                case "checkout.session.completed":
                    //await HandleCheckoutSessionCompleted(stripeEvent);
                    break;
                case "charge.dispute.closed":
                    //await HandleDisputeClosed(stripeEvent);
                    break;

                case "charge.refunded":
                case "charge.refund.updated":
                case "charge.refund.created":
                    //await HandleStripeRefundEvent(stripeEvent);
                    break;

                case "customer.subscription.created":
                case "customer.subscription.updated":
                case "customer.subscription.deleted":
                case "customer.subscription.pending_update_applied":
                case "customer.subscription.pending_update_expired":
                    await HandleSubscriptionProcessUpdate(stripeSubscription, stripeEvent);
                    break;
            }

            // 3. MARK PROCESSED
            await _eventsRepo.MarkStripeEventProcessedAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe event: " + stripeEvent.Id);
            throw; // Let Stripe retry
        }
    }

    private async Task HandleSubscriptionProcessUpdate(Subscription sub, Event stripeEvent)
    {
        var item = sub.Items.Data[0];
        var meta = sub.Metadata;

        // Safe metadata extraction
        var userId = Guid.Parse(meta["user_id"]);
        var planPriceId = Guid.Parse(meta["plan_price_id"]);
        var updatedBy = meta.ContainsKey("updated_by") ? meta["updated_by"] : "stripe";


        // Convert Unix timestamps safely
        DateTime? periodStart = item.CurrentPeriodStart;
        DateTime? periodEnd = item.CurrentPeriodEnd;
        DateTime? canceledAt = sub.CanceledAt;
        DateTime? trialStart = sub.TrialStart;
        DateTime? trialEnd = sub.TrialEnd;
        var metadataJson = JsonConvert.SerializeObject(new
        {
            user_id = sub.Metadata["user_id"],
            plan_price_id = sub.Metadata["plan_price_id"],
            updated_by = sub.Metadata["updated_by"],
            subscription_id = sub.Id,
            status = sub.Status,
            event_type = stripeEvent.Type,
            period_start = periodStart,
            period_end = periodEnd,
            cancel_at_period_end = sub.CancelAtPeriodEnd,
            canceled_at = canceledAt,
            trial_start = trialStart,
            trial_end = trialEnd
        });
        var anySubscriptionInvoiceDB = new SubscriptionEventProcessDto
        {

            stripe_subscription_id=sub.Id,
            user_id = Guid.Parse(sub.Metadata["user_id"]),
            plan_price_id = Guid.Parse(sub.Metadata["plan_price_id"]),
            status = sub.Status,
            current_period_start = periodStart,
            current_period_end = periodEnd,
            cancel_at_period_end=sub.CancelAtPeriodEnd,
            canceled_at = canceledAt,
            //trial_start = sub.TrialStart?.TrialStart,
            trial_end = trialEnd,
            stripe_event_id = stripeEvent.Id,
            event_type = stripeEvent.Type,
            metadata_json = metadataJson,
            actor="stripe_webhook",
            source="stripe"
        };
        await _subscriptionsRepo.SubscriptionEventProcessUpdateAsync(anySubscriptionInvoiceDB);
    }   
    
    private async Task HandleInvoiceEventProcessUpdate(Invoice stripeInvoice, Event stripeEvent)
    {
        if (stripeInvoice == null)
            return;

        // ------------------------------------------------------------
        // 1. Metadata validation
        // ------------------------------------------------------------
        var inv_metadata = stripeInvoice.Lines.Data[0].Metadata;

        if (!inv_metadata.ContainsKey("user_id") ||
            !inv_metadata.ContainsKey("plan_price_id"))
            return;

        var user_id = Guid.Parse(inv_metadata["user_id"]);
        var plan_price_id = Guid.Parse(inv_metadata["plan_price_id"]);
        

        // ------------------------------------------------------------
        // 2. Load subscription from DB
        // ------------------------------------------------------------
        //var stripeSubId = stripeInvoice.;
        var subscription = await _subscriptionsRepo.GetActiveSubscriptionForUserAsync(user_id);
        //if (subscription == null)
        //{
           // return;           
        //}
          

        // ------------------------------------------------------------
        // 3. Invoice type (your business classification)
        // ------------------------------------------------------------
        var invoice_type = inv_metadata.ContainsKey("subscription_type")
            ? inv_metadata["subscription_type"]
            : "main";

        // ------------------------------------------------------------
        // 4. Stripe invoice classification
        // ------------------------------------------------------------
        var billingReason = stripeInvoice.BillingReason;
        bool isFirstInvoice = billingReason == "subscription_create";
        bool isRenewal = billingReason == "subscription_cycle";
        bool isProration = billingReason == "subscription_update";

        // ------------------------------------------------------------
        // 5. Business invoice classification
        // ------------------------------------------------------------
        bool isBundleInvoice = inv_metadata.ContainsKey("bundle_id");
        bool isJobInvoice = inv_metadata.ContainsKey("job_id");
        var entity_id = subscription.id;
        var entity_type = inv_metadata.ContainsKey("source_type")
                ? inv_metadata["source_type"]
                : "subscription";



        if (entity_type != "subscription")
        {
            entity_id = Guid.Parse("");
        }
        switch (invoice_type)
        {
            case "main":
            case "trade_category":
            case "mobile_message":
            case "premium":
            case "enterprise":
                break;

            case "bundle":
            case "job":
            default:
                return;
        }
      
        // ------------------------------------------------------------
        // 6. Build metadata JSON
        // ------------------------------------------------------------
        var metadataJson = JsonConvert.SerializeObject(new
        {
            user_id = user_id,
            plan_price_id = plan_price_id,
            stripe_invoice_id = stripeInvoice.Id,
            status = stripeInvoice.Status,
            period_start = stripeInvoice.PeriodStart,
            period_end = stripeInvoice.PeriodEnd,
            event_type = stripeEvent.Type,
            updated_by = inv_metadata.ContainsKey("updated_by")
                ? inv_metadata["updated_by"]
                : "stripe"
        });

        // ------------------------------------------------------------
        // 7. Convert invoice lines → List<InvoiceItemCreateDto>
        // ------------------------------------------------------------
        var items = stripeInvoice.Lines.Data.Select(line => new InvoiceItemCreateDto
        {
            entity_type = entity_type,
            entity_id = entity_id,
            description = line.Description,
            quantity = (int)line.Quantity,
            unit_price = (line.Amount / 100m) / (line.Quantity ?? 1),
            total_price = line.Amount / 100m
        }).ToList();

        // Period start
        DateTime? paid_at = stripeInvoice.StatusTransitions?.PaidAt;

        var inv_Event_dto = new InvoiceEventProcessDto
        {
            user_id = user_id,
            plan_price_id = plan_price_id,
            subscription_id = subscription.id,
            stripe_invoice_id = stripeInvoice.Id,
            stripe_payment_intent_id = "",
            status = stripeInvoice.Status,
            invoice_type = invoice_type,
            currency = stripeInvoice.Currency?.ToUpper(),
            subtotal = stripeInvoice.Subtotal / 100m,
            tax_amount = (stripeInvoice.TotalTaxes?.Sum(x => x.Amount) ?? 0) / 100m,
            discount_amount = (stripeInvoice.TotalDiscountAmounts?.Sum(x => x.Amount) ?? 0) / 100m,
            total_amount = stripeInvoice.Total / 100m,
            billing_email = stripeInvoice.CustomerEmail,
            billing_period_start = stripeInvoice?.PeriodStart,
            billing_period_end = stripeInvoice?.PeriodEnd,
            issued_at = stripeInvoice.Created,
            paid_at = stripeInvoice.StatusTransitions?.PaidAt,
            due_at = stripeInvoice.DueDate,
            metadata_json = metadataJson,
            stripe_event_id = stripeEvent.Id,
            event_type = stripeEvent.Type,
            actor = "stripe_webhook",
            source = "stripe",          
            Items = items
        };

        // ------------------------------------------------------------
        // 11. Save to DB
        // ------------------------------------------------------------
        await _invoicesRepo.Invoice_event_process_updateAsync(inv_Event_dto);
    }
    private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        var metadata = intent.Metadata;
        var source_type = metadata.ContainsKey("source_type")
                ? metadata["source_type"]
                : "subscription";
        switch (source_type)
        {
            case "credit_bundle":               
                break;
            case "bundle":
                return;

            case "job":
                return;

            default:
                return;
        }
        var user_id = Guid.Parse(metadata["user_id"]);
        var bundle_order_id = Guid.Parse(metadata["bundle_order_id"]);
        var bundle_price_id = Guid.Parse(metadata["plan_price_id"]);

        var metadataJson = JsonConvert.SerializeObject(new
        {
            user_id = user_id,
            plan_price_id = bundle_price_id,            
            status = intent.Status,         
            event_type = stripeEvent.Type,
            updated_by = metadata.ContainsKey("updated_by")
                ? metadata["updated_by"]
                : "stripe"
        });

        var dto = new BundleCheckoutCompletedDto
        {

            bundle_order_id = Guid.Parse(metadata["bundle_order_id"]),
            bundle_price_id = Guid.Parse(metadata["plan_price_id"]),
            user_id = Guid.Parse(metadata["user_id"]),
            stripe_payment_intent_id = intent.Id,
            stripe_customer_id = intent.CustomerId,
            customer_email = intent.ReceiptEmail,
            amount_total = intent.AmountReceived / 100m,
            amount_subtotal = intent.Amount / 100m,
            currency = intent.Currency,
            metadataJson= metadataJson
        };
        await _bundlePurchase.OnBundleCheckoutCompletedAsync(dto);

    }
    // =====================================================================
    // 0b. MANUAL / API REFUND ENTRYPOINT (unified pipeline)
    // =====================================================================
    public async Task HandleRefundAsync(
        string stripe_payment_intent_id,
        decimal amount,
        string reason,
        string reference_type,
        string reference_id,
        string user_id)
    {
        // Here you already know the mapping (admin / support tool).
        var userId = Guid.Parse(user_id);
        var refId = Guid.Parse(reference_id);
        var credits = (int)amount; // you can map £→credits differently if needed

        await RefundCreditsAsync(
            userId,
            credits,
            reference_type,
            refId,
            new
            {
                stripe_payment_intent_id,
                reason,
                source = "manual"
            });
    }

    // =====================================================================
    // 2. INVOICE FAILED
    // =====================================================================
    private async Task HandleInvoiceFailed(Event stripeEvent)
    {
        var obj = stripeEvent.Data.RawObject as JObject;
        if (obj == null) return;

        var stripe_invoiceid = obj["id"]?.ToString()!;
        var payment_intent_id = obj["payment_intent"]?.ToString();
        var subscription_id = obj["subscription"]?.ToString();

        await _invoicesRepo.MarkFailedAsync(stripe_invoiceid);

        if (!string.IsNullOrEmpty(payment_intent_id))
            await _payments.MarkFailedAsync(payment_intent_id);

        if (!string.IsNullOrEmpty(subscription_id))
            await _subscriptionsRepo.MarkPastDueAsync(subscription_id);
    }

    // =====================================================================
    // 3. BUNDLE CHECKOUT → grant credits
    // =====================================================================

    private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

        if (session == null)
        {
            _logger.LogError("checkout.session.completed received but object is not a Session");
            return;
        }

        var metadata = session.Metadata;

        var dto = new BundleCheckoutCompletedDto
        {
            bundle_order_id = Guid.Parse(metadata["bundle_order_id"]),
            bundle_price_id = Guid.Parse(metadata["bundle_price_id"]),
            user_id = Guid.Parse(metadata["user_id"]),

            stripe_payment_intent_id = session.PaymentIntentId,
            stripe_customer_id = session.CustomerId,
            customer_email = session.CustomerEmail,

            amount_total = (session.AmountTotal ?? 0) / 100m,
            amount_subtotal = (session.AmountSubtotal ?? 0) / 100m,
            currency = session.Currency
        };

        await _bundlePurchase.OnBundleCheckoutCompletedAsync(dto);
    }
 
    private async Task HandlePaymentIntentFailed(Event stripeEvent)
    {
        var intent = stripeEvent.Data.Object as PaymentIntent;
        var metadata = intent.Metadata;
        var dto = new BundleCheckoutFailedDto
        {
            bundle_order_id = Guid.Parse(metadata["bundle_order_id"])            
        };
        await _bundlePurchase.OnBundleOrderMarkFailedAsync(dto);
    }
    //private async Task HandleBundleCheckout(Event stripeEvent)
    //{
    //    var obj = stripeEvent.Data.RawObject as JObject;
    //    if (obj == null) return;

    //    var userId = Guid.Parse(obj["metadata"]?["user_id"]!.ToString());
    //    var credits = int.Parse(obj["metadata"]?["credits"]!.ToString());
    //    var paymentIntentId = Guid.Parse(obj["payment_intent"]!.ToString());

    //    await _creditService.GrantAsync(new CreditGrantRequest
    //    {
    //        user_id = userId,
    //        source = "bundle",
    //        reference_id = paymentIntentId,
    //        total_credits = credits,
    //        expires_at = DateTime.UtcNow.AddMonths(24),
    //        reference_type = "bundle",
    //        metadata = JsonConvert.SerializeObject(new
    //        {
    //            payment_intent = paymentIntentId
    //        })
    //    });
    //}

    // =====================================================================
    // 4. DISPUTE CLOSED → refund credits (via unified pipeline)
    // =====================================================================
    private async Task HandleDisputeClosed(Event stripeEvent)
    {
        var obj = stripeEvent.Data.RawObject as JObject;
        if (obj == null) return;

        var paymentIntentId = obj["payment_intent"]?.ToString();
        if (paymentIntentId == null) return;

        var userId = Guid.Parse(obj["metadata"]?["user_id"]!.ToString());
        var jobId = Guid.Parse(obj["metadata"]?["job_id"]!.ToString());
        var credits = int.Parse(obj["metadata"]?["credits"]!.ToString());

        await RefundCreditsAsync(
            userId,
            credits,
            "dispute",
            jobId,
            new
            {
                payment_intent = paymentIntentId,
                source = "dispute_closed"
            });
    }

   

    // =====================================================================
    // 6. SUBSCRIPTION DELETED
    // =====================================================================
    private async Task HandleSubscriptionDeleted(Event stripeEvent)
    {
        var sub = stripeEvent.Data.Object as Subscription;
        if (sub == null) return;

        await _subscriptionsRepo.MarkCanceledAsync(sub.Id);
    }

    // =====================================================================
    // 7. STRIPE REFUND EVENTS → unified refund pipeline
    // =====================================================================
    private async Task HandleStripeRefundEvent(Event stripeEvent)
    {
        var obj = stripeEvent.Data.RawObject as JObject;
        if (obj == null) return;

        var chargeId = obj["id"]?.ToString();
        if (chargeId == null)
            return;

        var metadata = obj["metadata"] as JObject;
        if (metadata == null)
            return;

        var userId = Guid.Parse(metadata["user_id"]!.ToString());
        var credits = int.Parse(metadata["credits"]!.ToString());
        var referenceId = Guid.Parse(metadata["reference_id"]!.ToString());
        var referenceType = metadata["reference_type"]!.ToString();
        var reason = metadata["reason"]?.ToString() ?? "Stripe refund issued";

        await RefundCreditsAsync(
            userId,
            credits,
            referenceType,
            referenceId,
            new
            {
                stripe_charge_id = chargeId,
                reason,
                source = "stripe_refund_event"
            });
    }

    // =====================================================================
    // 8. UNIFIED REFUND PIPELINE → single place that talks to CreditService
    // =====================================================================
    private async Task RefundCreditsAsync(
        Guid userId,
        int credits,
        string referenceType,
        Guid referenceId,
        object extraMetadata)
    {
        var metadataJson = JsonConvert.SerializeObject(extraMetadata);

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
}

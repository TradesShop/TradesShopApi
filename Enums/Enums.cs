using System.Collections.Generic;
using System.Linq;


namespace TradePlatform.Api.Enums
{
   
    public sealed class EventType
    {
        public int Id { get; }
        public string Code { get; }
        public string Name { get; }

        private EventType(int id, string code, string name)
        {
            Id = id;
            Code = code;
            Name = name;
        }

        
        public static readonly EventType MagicUrlClick =
            new EventType(1, "magicurl_click", "Magic URL Click");

        public static readonly EventType Login =
            new EventType(2, "login", "Login");

        public static readonly EventType Logout =
            new EventType(3, "logout", "Logout");

        public static readonly EventType JobPosted =
            new EventType(4, "job_posted", "Job Posted");

        public static readonly EventType JobPurchased =
            new EventType(5, "job_purchased", "Job Purchased");

        public static readonly EventType CommonMessage =
            new EventType(7, "common_message", "Common Message");

        public static readonly EventType ReviewRequested =
            new EventType(8, "review_requested", "Review Requested");

        public static readonly EventType ReviewPosted =
            new EventType(9, "review_posted", "Review Posted");

        public static readonly EventType ReviewReplied =
            new EventType(10, "review_replied", "Review Replied");

        public static readonly EventType JobDisputeSubmit =
            new EventType(11, "job_dispute_submit", "Job Dispute Submitted");

        public static readonly EventType JobDisputedMessage =
            new EventType(12, "job_disputed_message", "Job Disputed Message");

        public static readonly EventType JobDisputeAction =
            new EventType(13, "job_dispute_action", "Job Dispute Action");

        public static readonly EventType JobPurchasedMessage =
            new EventType(14, "job_purchase_message", "Job Purchased Message");
       
        public static readonly EventType InvoicePaid = new EventType(15, "invoice_paid", "Invoice Paid");

        public static readonly EventType SubscriptionAutoNewOn = new EventType(16, "subscription_auto_renew_enabled", "Subscription Auto Renew On");

        public static readonly EventType SubscriptionAutoNewOff = new EventType(17, "subscription_auto_renew_disabled", "Subscription Auto Renew OFF");

        public static readonly EventType SubscriptionUpdated = new EventType(18, "subscription_updated", "Subscription Updated");

        public static readonly EventType SubscriptionCancelled = new EventType(19, "subscription_cancelled", "Subscription Cancelled");

        public static readonly EventType SubscriptionScheduleCreated = new EventType(20, "subscription_schedule_created", "Subscription schedule created");

        public static readonly EventType SubscriptionScheduleUpdated = new EventType(21, "subscription_schedule_updated", "Subscription schedule Updated");

        public static readonly EventType SubscriptionScheduleCancelled = new EventType(22, "subscription_schedule_cancelled", "Subscription schedule Cancelled");

        public static readonly EventType SubscriptionScheduleExpiring = new EventType(23, "subscription_schedule_expiring", "Subscription schedule Expiring");

        public static readonly EventType RefundSucceeded = new EventType(24, "refund_succeeded", "Refund Succeeded");


        public static IEnumerable<EventType> All =>
            new[]
            {
            MagicUrlClick,
            Login,
            Logout,
            JobPosted,
            JobPurchased,
            CommonMessage,
            ReviewRequested,
            ReviewPosted,
            ReviewReplied,
            JobDisputeSubmit,
            JobDisputedMessage,
            JobDisputeAction,
            JobPurchasedMessage,
            InvoicePaid,
            SubscriptionAutoNewOn,
            SubscriptionAutoNewOff,
            SubscriptionUpdated,
            SubscriptionCancelled,
            SubscriptionScheduleCreated,
            SubscriptionScheduleUpdated,
            SubscriptionScheduleCancelled,
            RefundSucceeded

            };

        // -------------------------
        // Lookup helpers
        // -------------------------
        public static EventType FromId(int id) =>
            All.FirstOrDefault(x => x.Id == id);

        public static EventType FromCode(string code) =>
            All.FirstOrDefault(x => x.Code == code);
    }



    public sealed class EntityType
    {
        public int Id { get; }
        public string Code { get; }
        public string Name { get; }

        private EntityType(int id, string code, string name)
        {
            Id = id;
            Code = code;
            Name = name;
        }

        // -------------------------
        // Static instances
        // -------------------------
        public static readonly EntityType User =
            new EntityType(1, "user", "user");

        public static readonly EntityType JobPosts =
            new EntityType(2, "job_posts", "job_post");

        public static readonly EntityType Subscription =
            new EntityType(3, "subscription", "subscription");

        public static readonly EntityType Usage =
            new EntityType(4, "usage", "usage");

        public static readonly EntityType CreditAdmin =
            new EntityType(5, "credit_admin", "credit_admin");

        public static readonly EntityType System =
            new EntityType(6, "system", "system");

        public static readonly EntityType Refund =
            new EntityType(7, "refund", "refund");

        public static readonly EntityType Promotion =
            new EntityType(8, "promotion", "promotion");

        public static readonly EntityType JobPurchase =
            new EntityType(9, "job_purchase", "job_purchase");

        public static readonly EntityType JobDispute =
            new EntityType(10, "job_dispute", "job_dispute");

        public static readonly EntityType Business =
            new EntityType(11, "business", "business");

        public static readonly EntityType CreditBundle =
            new EntityType(12, "credit_bundle", "credit_bundle");

        public static readonly EntityType ChatMessage =
            new EntityType(13, "chatmessage", "chatmessage");

        public static readonly EntityType MagicUrlClick =
            new EntityType(14, "magicurl_click", "magicurl_click");

        public static readonly EntityType JobReview = new EntityType(15, "job_review", "job reviews");

        public static readonly EntityType SupportTicket = new EntityType(16, "support_ticket", "support_ticket");

        public static readonly EntityType RefundRequest = new EntityType(17, "refund_request", "refund_request");

        public static readonly EntityType Invoice = new EntityType(18, "invoice", "invoice_paid");

        public static readonly EntityType SubscriptionPending = new EntityType(19, "subscription_pending", "subscription_pending");


        // -------------------------
        // List of all types
        // -------------------------
        public static IEnumerable<EntityType> All =>
            new[]
            {
            User,
            JobPosts,
            Subscription,
            Usage,
            CreditAdmin,
            System,
            Refund,
            Promotion,
            JobPurchase,
            JobDispute,
            Business,
            CreditBundle,
            ChatMessage,
            MagicUrlClick,
            JobReview,
            SupportTicket,
            RefundRequest,
            Invoice,
            SubscriptionPending
            };

        // -------------------------
        // Lookup helpers
        // -------------------------
        public static EntityType FromId(int id) =>
            All.FirstOrDefault(x => x.Id == id);

        public static EntityType FromCode(string code) =>
            All.FirstOrDefault(x => x.Code == code);
    }

   
}

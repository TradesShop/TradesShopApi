using Stripe;
using System;
using System.Threading.Tasks;
using TradePlatform.Api.DTOs.Stripe;
using TradePlatform.Api.DTOs.subscription;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Subscriptions
{
    public class UserSubscriptionService: IUserSubscriptionService
    {
        private readonly ISubscriptionsRepository _repoUsubscription;
       

        public UserSubscriptionService(
            ISubscriptionsRepository repoUsubscription
            )
        {
            _repoUsubscription = repoUsubscription;
            
        }

        public async Task<SubscriptionViewDto> GetActiveSubscriptionForUserAsync(Guid user_id)
        {
            return  await _repoUsubscription.GetActiveSubscriptionForUserAsync(user_id);
           
        }

        //public async Task<SubscriptionDto> CreateOrUpdateSubscriptionAsync(Guid user_id, string price_id, Guid? updated_by)
        //{
        //    var primary = await _pmRepo.GetPrimaryForUserAsync(user_id);
        //    if (primary == null)
        //        throw new Exception("User has no primary payment method.");

        //    var stripeSub = await _stripe.CreateOrUpdateSubscriptionAsync(
        //        user_id,
        //        price_id,
        //        primary.stripe_paymentmethod_id
        //    );

        //    var existing = await _repo.GetByStripeIdAsync(stripeSub.Id);
        //    var now = DateTime.UtcNow;

        //    if (existing == null)
        //    {
        //        existing = new Subscriptions
        //        {
        //            id = Guid.NewGuid(),
        //            user_id = user_id,
        //            stripe_subscriptionid = stripeSub.Id,
        //            stripe_priceid = price_id,
        //            status = stripeSub.Status,
        //            periodstart = stripeSub.StartDate,
        //            periodend = stripeSub.EndedAt,
        //            created_at = now,
        //            updated_at = now,
        //            updated_by = updated_by
        //        };

        //        await _repo.InsertAsync(existing);
        //    }
        //    else
        //    {
        //        existing.stripe_priceid = price_id;
        //        existing.status = stripeSub.Status;
        //        existing.periodstart = stripeSub.StartDate;
        //        existing.periodend = stripeSub.EndedAt;
        //        existing.updated_at = now;
        //        existing.updated_by = updated_by;

        //        await _repo.UpdateAsync(existing);
        //    }

        //    return new SubscriptionDto
        //    {
        //        id = existing.id,
        //        stripe_subscriptionid = existing.stripe_subscriptionid,
        //        stripe_priceid = existing.stripe_priceid,
        //        status = existing.status,
        //        periodstart = existing.periodstart,
        //        periodend = existing.periodend
        //    };
        //}

        //public async Task CancelSubscriptionAsync(Guid user_id, Guid? updated_by)
        //{
        //    var sub = await _repo.GetActiveByUserAsync(user_id);
        //    if (sub == null)
        //        return;

        //    await _stripe.CancelSubscriptionAsync(sub.stripe_subscriptionid);

        //    sub.status = "canceled";
        //    sub.updated_at = DateTime.UtcNow;
        //    sub.updated_by = updated_by;

        //    await _repoUsubscription.UpdateAsync(sub);
        //}
    }
}

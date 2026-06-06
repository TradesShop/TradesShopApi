using TradePlatform.Api.DTOs.Bundles;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Helpers
{
    public static class BundlesMappingHelper
    {
        // ------------------------------------------------------------
        // MODEL → DTO
        // ------------------------------------------------------------

        public static BundleDto ToDto(CreditBundles model)
        {
            return new BundleDto
            {
                //Id = model.id,
                Name = model.name,
                Description = model.description,
                ExpiryMonths = model.expiry_months,
                IsActive = model.is_active
            };
        }

        public static BundlePriceDto ToDto(BundlePrices model)
        {
            return new BundlePriceDto
            {
                Id = model.id,
                BundleId = model.bundle_id,
                Price = model.price,
                Currency = model.currency,
                StripePriceId = model.stripe_price_id,
                IsActive = model.is_active,
                IsVatable = model.is_vatable
            };
        }

        public static BundleOrderDto ToDto(BundleOrders model)
        {
            return new BundleOrderDto
            {
                Id = model.id,
                UserId = model.user_id,
                Bundle_Price_Id = model.bundle_price_id,
                StripeSessionId = model.stripe_session_id,
                StripePaymentIntentId = model.stripe_payment_intent_id,
                StripePriceId = model.stripe_price_id,
                Amount = model.amount,
                Currency = model.currency,
                Status = model.status,
                CreatedAt = model.created_at
            };
        }

        // ------------------------------------------------------------
        // DTO → MODEL (Admin Create)
        // ------------------------------------------------------------

        public static CreditBundles ToModel(BundleCreateDto dto)
        {
            return new CreditBundles
            {
                id = Guid.NewGuid(),
                name = dto.Name,
                description = dto.Description,
                expiry_months = dto.ExpiryMonths,
                is_active = dto.IsActive,
                created_at = DateTime.UtcNow
            };
        }

        public static BundlePrices ToModel(BundlePriceCreateDto dto, Guid bundleId)
        {
            return new BundlePrices
            {
                id = Guid.NewGuid(),
                bundle_id = bundleId,
                price = dto.Price,
                currency = dto.Currency,
                stripe_price_id = dto.StripePriceId,
                is_active = true,
                is_vatable = dto.IsVatable,
                created_at = DateTime.UtcNow
            };
        }

        // ------------------------------------------------------------
        // DTO → MODEL (Admin Update)
        // ------------------------------------------------------------

        public static void ApplyUpdate(CreditBundles model, BundleUpdateDto dto)
        {
            model.name = dto.Name;
            //model.credits = dto.Credits;
            model.expiry_months = dto.ExpiryMonths;
            model.is_active = dto.IsActive;
        }

        public static void ApplyUpdate(BundlePrices model, BundlePriceUpdateDto dto)
        {
            model.is_active = dto.IsActive;
            model.is_vatable = dto.IsVatable;
        }
    }
}

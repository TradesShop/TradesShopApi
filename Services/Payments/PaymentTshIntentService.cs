using Stripe;
using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.DTOs.Payments;
using TradePlatform.Api.Helpers;
using TradePlatform.Api.Models;
using TradePlatform.Api.Repositories.Interfaces;

namespace TradePlatform.Api.Services.Payments
{
    public class PaymentTshIntentService : IPaymentTshIntentService
    {
        private readonly IInvoicesTshRepository _invoicesRepo;
        private readonly IInvoiceItemsRepository _itemsRepo;
        private readonly IConfiguration _config;

        public PaymentTshIntentService(
            IInvoicesTshRepository invoicesRepo,
            IInvoiceItemsRepository itemsRepo,
            IConfiguration config)
        {
            _invoicesRepo = invoicesRepo;
            _itemsRepo = itemsRepo;
            _config = config;
        }

        //public async Task<StartPaymentResponseDto> StartPaymentAsync(StartPaymentRequestDto dto)
        //{
        //    var now = DateTime.UtcNow;

        //    var invoiceItems = dto.Items.Select(i =>
        //    {
        //        var total = i.UnitPrice * i.Quantity;
        //        return new InvoiceItems
        //        {
        //            id = Guid.NewGuid(),
        //            invoice_id = Guid.Empty, // temp, set after invoice created
        //            reference_type = i.ReferenceType,
        //            reference_id = i.ReferenceId,
        //            description = i.Description,
        //            quantity = i.Quantity,
        //            unit_price = i.UnitPrice,
        //            total_price = total,
        //            metadata = i.Metadata,
        //            created_at = now
        //        };
        //    }).ToList();

        //    var subtotal = invoiceItems.Sum(x => x.total_price);
        //    var tax = 0m;
        //    var discount = 0m;
        //    var total = subtotal + tax - discount;

        //    var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        //    var invoiceModel = InvoicesMappingHelper.ToModel(
        //        new InvoiceEventProcessDto
        //        {
        //            UserId = dto.UserId,
        //            Type = dto.Type,
        //            Currency = dto.Currency,
        //            BillingEmail = dto.BillingEmail,
        //            IssuedAt = now,
        //            DueAt = null,
        //            Items = dto.Items
        //        },
        //        dto.UserId,
        //        invoiceNumber,
        //        subtotal,
        //        tax,
        //        discount,
        //        total
        //    );

        //    await _invoicesRepo.InsertInvoiceAsync(invoiceModel);

        //    foreach (var item in invoiceItems)
        //        item.invoice_id = invoiceModel.id;

        //    await _itemsRepo.CreateManyAsync(invoiceItems);

        //    StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        //    var paymentIntentService = new Stripe.PaymentIntentService();
        //    var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
        //    {
        //        Amount = (long)(total * 100),
        //        Currency = dto.Currency,
        //        ReceiptEmail = dto.BillingEmail,
        //        Metadata = new Dictionary<string, string>
        //        {
        //            { "invoice_id", invoiceModel.id.ToString() },
        //            { "invoice_number", invoiceModel.invoice_number }
        //        }
        //    });

        //    invoiceModel.stripe_payment_intent_id = paymentIntent.Id;
        //    invoiceModel.updated_at = DateTime.UtcNow;
        //    await _invoicesRepo.UpdateAsync(invoiceModel);

        //    return new StartPaymentResponseDto
        //    {
        //        InvoiceId = invoiceModel.id,
        //        InvoiceNumber = invoiceModel.invoice_number,
        //        StripePaymentIntentId = paymentIntent.Id,
        //        ClientSecret = paymentIntent.ClientSecret
        //    };
        //}
    }
}

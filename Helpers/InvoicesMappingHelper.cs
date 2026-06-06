using TradePlatform.Api.DTOs.Invoices;
using TradePlatform.Api.Models;

namespace TradePlatform.Api.Helpers
{
    public static class InvoicesMappingHelper
    {
        // ------------------------------------------------------------
        // MODEL → DTO (Invoice)
        // ------------------------------------------------------------
        //public static InvoiceDto ToDto(Invoices m, IEnumerable<InvoiceItems> items)
        //{
        //    return new InvoiceDto
        //    {
        //        Id = m.id,
        //        UserId = m.user_id,
        //        InvoiceNumber = m.invoice_number,
        //        Type = m.type,
        //        Status = m.status,
        //        Currency = m.currency,
        //        Subtotal = m.subtotal,
        //        TaxAmount = m.tax_amount,
        //        DiscountAmount = m.discount_amount,
        //        TotalAmount = m.total_amount,
        //        StripeInvoiceId = m.stripe_invoice_id,
        //        StripePaymentIntentId = m.stripe_payment_intent_id,
        //        StripeCustomerId = m.stripe_customer_id,
        //        BillingEmail = m.billing_email,
        //        IssuedAt = m.issued_at,
        //        PaidAt = m.paid_at,
        //        DueAt = m.due_at,
        //        CreatedAt = m.created_at,
        //        UpdatedAt = m.updated_at,
        //        Items = items.Select(ToDto).ToList()
        //    };
        //}

        // ------------------------------------------------------------
        // MODEL → DTO (Invoice Item)
        // ------------------------------------------------------------
        //public static InvoiceItemDto ToDto(InvoiceItems i)
        //{
        //    return new InvoiceItemDto
        //    {
        //        Id = i.id,
        //        InvoiceId = i.invoice_id,
        //        ReferenceType = i.reference_type,
        //        ReferenceId = i.reference_id,
        //        Description = i.description,
        //        Quantity = i.quantity,
        //        UnitPrice = i.unit_price,
        //        TotalPrice = i.total_price,
        //        Metadata = i.metadata,
        //        CreatedAt = i.created_at
        //    };
        //}

        // ------------------------------------------------------------
        // DTO → MODEL (Invoice)
        // Used when creating invoices
        // ------------------------------------------------------------
        //public static Invoices ToModel(
        //    InvoiceEventProcessDto dto,
        //    Guid userId,
        //    string invoiceNumber,
        //    decimal subtotal,
        //    decimal tax,
        //    decimal discount,
        //    decimal total)
        //{
        //    return new Invoices
        //    {
        //        id = Guid.NewGuid(),
        //        user_id = userId,
        //        invoice_number = invoiceNumber,
        //        type = dto.Type,
        //        status = "pending",
        //        currency = dto.Currency,
        //        subtotal = subtotal,
        //        tax_amount = tax,
        //        discount_amount = discount,
        //        total_amount = total,
        //        billing_email = dto.BillingEmail,
        //        issued_at = dto.IssuedAt,
        //        due_at = dto.DueAt,
        //        created_at = DateTime.UtcNow,
        //        updated_at = DateTime.UtcNow
        //    };
        //}

        // ------------------------------------------------------------
        // DTO → MODEL (Invoice Item)
        // ------------------------------------------------------------
        //public static InvoiceItems ToModel(
        //    InvoiceItemCreateDto dto,
        //    Guid invoiceId,
        //    decimal totalPrice)
        //{
        //    return new InvoiceItems
        //    {
        //        id = Guid.NewGuid(),
        //        invoice_id = invoiceId,
        //        reference_type = dto.ReferenceType,
        //        reference_id = dto.ReferenceId,
        //        description = dto.Description,
        //        quantity = dto.Quantity,
        //        unit_price = dto.UnitPrice,
        //        total_price = totalPrice,
        //        metadata = dto.Metadata,
        //        created_at = DateTime.UtcNow
        //    };
        //}
    }
}

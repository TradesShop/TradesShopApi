using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Tax;
using TradePlatform.Api.DTOs.Bundles;
using TradePlatform.Api.Exceptions;
using TradePlatform.Api.Models;
using TradePlatform.Api.Models.BundleCredit;
using TradePlatform.Api.Models.Plans;
using TradePlatform.Api.Repositories.Interfaces;
using TradePlatform.Api.Services.Subscriptions;

namespace TradePlatform.Api.Services.Bundles;

public class BundlePurchaseService : IBundlePurchaseService
{
	private readonly PaymentIntentService _paymentIntentService;

	private readonly IBundlesCreditRepository _bundlesRepo;

	private readonly IPaymentMethodRepository _paymentMethodRepository;

	private readonly IBundlePricesRepository _pricesRepo;

	private readonly IBundleOrdersRepository _ordersRepo;

	private readonly IStripeService _stripeservice;

	private readonly StripeClient _stripeClient;

	private readonly ILogger<BundlePurchaseService> _logger;

	private readonly IIdentityService _identityService;

	private readonly IPlansRepository _plansRepository;

	private readonly IBusinessRepository _businessRepo;

	private readonly IUserSubscriptionService _uSubsctriptionService;

	public BundlePurchaseService(PaymentIntentService paymentIntentService, IBundlesCreditRepository bundlesRepo, IPaymentMethodRepository paymentMethodRepository, IBundlePricesRepository pricesRepo, IBundleOrdersRepository ordersRepo, IStripeService stripeservice, ILogger<BundlePurchaseService> logger, IIdentityService identityService, IPlansRepository plansRepository, IBusinessRepository businessRepo, IUserSubscriptionService uSubsctriptionService, StripeClient stripeClient)
	{
		_paymentIntentService = paymentIntentService;
		_bundlesRepo = bundlesRepo;
		_paymentMethodRepository = paymentMethodRepository;
		_pricesRepo = pricesRepo;
		_ordersRepo = ordersRepo;
		_stripeservice = stripeservice;
		_logger = logger;
		_identityService = identityService;
		_plansRepository = plansRepository;
		_businessRepo = businessRepo;
		_uSubsctriptionService = uSubsctriptionService;
		_stripeClient = stripeClient;
	}

	public async Task<BundlePurchaseResponse> CreateCheckoutSessionAsync(Guid userId, Guid bundleId, Guid bundle_price_id, CancellationToken cancellationToken)
	{
		PlanPriceByPriceId anyCredit = await _plansRepository.GetPlanPriceByPriceId(bundle_price_id);
		if (anyCredit == null)
		{
			throw new BusinessException("Bundle/Plan credit not found.");
		}
		_ = anyCredit.price;
		if (anyCredit.plan_id != bundleId)
		{
			throw new BusinessException("Invalid bundle price.");
		}
		if (!anyCredit.is_active)
		{
			throw new BusinessException("Bundle price is not active.");
		}
		if (string.IsNullOrWhiteSpace(anyCredit.stripe_price_id))
		{
			throw new BusinessException("Bundle credit does not have a Stripe price ID.");
		}
		string stripeCustomerId = await _stripeservice.ResolveStripeCustomerIdAsync(userId);
		await _uSubsctriptionService.SyncStripeCustomerAddressAsync(userId, stripeCustomerId);
		if (string.IsNullOrWhiteSpace(stripeCustomerId))
		{
			throw new BusinessException("Stripe customer not found.");
		}
		PaymentMethod_db defaultPaymentMethod = await _paymentMethodRepository.GetDefaultPaymentMethodAsync(userId);
		if (defaultPaymentMethod == null || string.IsNullOrWhiteSpace(defaultPaymentMethod.stripe_payment_method_id))
		{
			return new BundlePurchaseResponse
			{
				success = true,
				bundle_order_id = null,
				message = "No default payment method found.",
				requires_payment_method = true
			};
		}
		string paymentMethodId = defaultPaymentMethod.stripe_payment_method_id;
		BundleOrders order = new BundleOrders
		{
			user_id = userId,
			bundle_price_id = bundle_price_id,
			stripe_price_id = anyCredit.stripe_price_id,
			amount = anyCredit.price,
			currency = anyCredit.currency,
			status = "pending",
			created_at = DateTime.UtcNow
		};
		BundleOrders anyorder = await _ordersRepo.CreateAsync(order);
		_logger.LogInformation("Bundle order created. OrderId: {OrderId}", anyorder.id);
		Dictionary<string, string> metadata = new Dictionary<string, string>
		{
			{
				"user_id",
				userId.ToString()
			},
			{
				"bundle_order_id",
				anyorder.id.ToString()
			},
			{
				"entity_id",
				anyorder.id.ToString()
			},
			{ "entity_type_id", "12" },
			{ "entity_type", "credit_bundle" },
			{
				"plan_price_id",
				bundle_price_id.ToString()
			},
			{ "source_type", "credit_bundle" },
			{ "action", "creditbundle_created" },
			{
				"updated_by",
				_identityService.GetUserId().ToString()
			},
			{ "reason", "User purchased credit bundle" }
		};
		InvoiceService invoiceService = new InvoiceService(_stripeClient);
		InvoiceCreateOptions invoiceOptions = new InvoiceCreateOptions
		{
			Customer = stripeCustomerId,
			Currency = anyCredit.currency,
			CollectionMethod = "charge_automatically",
			DefaultPaymentMethod = paymentMethodId,
			AutoAdvance = false,
			AutomaticTax = new InvoiceAutomaticTaxOptions
			{
				Enabled = true
			},
			Description = $"Credit Bundle Purchase - Order {anyorder.id}",
			Metadata = metadata
		};
		Invoice invoice;
		try
		{
			RequestOptions invoiceRequestOptions = new RequestOptions
			{
				IdempotencyKey = $"{anyorder.id}-invoice"
			};
			invoice = await invoiceService.CreateAsync(invoiceOptions, invoiceRequestOptions, cancellationToken);
		}
		catch (StripeException exception)
		{
			_logger.LogError(exception, "Stripe invoice creation failed. OrderId: {OrderId}", anyorder.id);
			throw new BusinessException("Unable to create invoice. Please try again.");
		}
		_logger.LogInformation("Stripe invoice created. InvoiceId: {InvoiceId}, OrderId: {OrderId}", invoice.Id, anyorder.id);
		InvoiceItemService invoiceItemService = new InvoiceItemService(_stripeClient);
		InvoiceItemCreateOptions invoiceItemOptions = new InvoiceItemCreateOptions
		{
			Customer = stripeCustomerId,
			Invoice = invoice.Id,
			Amount = (long)Math.Round(anyCredit.price * 100m, MidpointRounding.AwayFromZero),
			Currency = anyCredit.currency,
			Description = $"Credit Bundle - {anyCredit.credits_per_period} Credits",
			TaxBehavior = "exclusive",
			Metadata = metadata
		};
		try
		{
			RequestOptions invoiceItemRequestOptions = new RequestOptions
			{
				IdempotencyKey = $"{anyorder.id}-invoice-item"
			};
			await invoiceItemService.CreateAsync(invoiceItemOptions, invoiceItemRequestOptions, cancellationToken);
		}
		catch (StripeException ex)
		{
			_logger.LogError(ex, "Stripe invoice item creation failed. InvoiceId: {InvoiceId}, OrderId: {OrderId}, StripeCode: {StripeCode}, StripeMessage: {StripeMessage}, StripeParam: {StripeParam}", invoice.Id, anyorder.id, ex.StripeError?.Code, ex.StripeError?.Message, ex.StripeError?.Param);
			try
			{
				await invoiceService.DeleteAsync(invoice.Id, null, null, cancellationToken);
			}
			catch (Exception exception2)
			{
				_logger.LogError(exception2, "Failed to cleanup Stripe invoice {InvoiceId}", invoice.Id);
			}
			throw new BusinessException("Unable to create invoice item. Please try again.");
		}
		Invoice finalizedInvoice;
		try
		{
			finalizedInvoice = await invoiceService.FinalizeInvoiceAsync(invoice.Id, null, null, cancellationToken);
		}
		catch (StripeException exception3)
		{
			_logger.LogError(exception3, "Stripe invoice finalization failed. InvoiceId: {InvoiceId}, OrderId: {OrderId}", invoice.Id, anyorder.id);
			throw new BusinessException("Unable to finalize invoice. Please try again.");
		}
		decimal amountSubtotal = (decimal)(finalizedInvoice.SubtotalExcludingTax ?? finalizedInvoice.Subtotal) / 100m;
		decimal amountTotal = (decimal)finalizedInvoice.Total / 100m;
		decimal vatAmount = ((decimal?)finalizedInvoice.TotalTaxes?.Sum((InvoiceTotalTax x) => x.Amount) / (decimal?)100m).GetValueOrDefault();
		_logger.LogInformation("Stripe invoice finalized. InvoiceId: {InvoiceId}, Subtotal: {Subtotal}, VAT: {VAT}, Total: {Total}", finalizedInvoice.Id, amountSubtotal, vatAmount, amountTotal);
		Invoice paidInvoice;
		try
		{
			InvoicePayOptions payOptions = new InvoicePayOptions
			{
				PaymentMethod = paymentMethodId,
				OffSession = true
			};
			RequestOptions payRequestOptions = new RequestOptions
			{
				IdempotencyKey = $"{anyorder.id}-invoice-payment"
			};
			paidInvoice = await invoiceService.PayAsync(finalizedInvoice.Id, payOptions, payRequestOptions, cancellationToken);
		}
		catch (StripeException exception4)
		{
			_logger.LogError(exception4, "Stripe invoice payment failed. InvoiceId: {InvoiceId}, OrderId: {OrderId}", finalizedInvoice.Id, anyorder.id);
			throw new BusinessException("Payment failed. Please try again.");
		}
		InvoiceGetOptions invoiceGetOptions = new InvoiceGetOptions
		{
			Expand = new List<string> { "payments.data.payment.payment_intent" }
		};
		Invoice finalInvoice = await invoiceService.GetAsync(paidInvoice.Id, invoiceGetOptions);
		InvoicePayment invoicePayment = finalInvoice.Payments?.Data?.FirstOrDefault((InvoicePayment x) => x.Status == "paid" && x.Payment?.Type == "payment_intent");
		if (invoicePayment?.Payment != null)
		{
			_ = invoicePayment.Payment.PaymentIntentId;
		}
		return new BundlePurchaseResponse
		{
			success = (finalInvoice.Status == "paid"),
			bundle_order_id = anyorder.id,
			message = ((finalInvoice.Status == "paid") ? "Payment completed successfully." : "Payment is being processed."),
			requires_action = false,
			client_secret = null
		};
	}

	public async Task<BundlePurchaseResponse> CreateCheckoutSessionAsync_v1(Guid userId, Guid bundleId, Guid bundle_price_id, CancellationToken cancellationToken)
	{
		PlanPriceByPriceId anyCredit = await _plansRepository.GetPlanPriceByPriceId(bundle_price_id);
		if (anyCredit == null)
		{
			throw new BusinessException("Bundle/Plan credit not found.");
		}
		_ = anyCredit.price;
		if (anyCredit.plan_id != bundleId)
		{
			throw new BusinessException("Invalid bundle price.");
		}
		if (!anyCredit.is_active)
		{
			throw new BusinessException("Bundle price is not active.");
		}
		if (string.IsNullOrEmpty(anyCredit.stripe_product_id))
		{
			throw new BusinessException("Bundle credit does not stripe product id.");
		}
		string stripeCustomerId = await _stripeservice.ResolveStripeCustomerIdAsync(userId);
		if (string.IsNullOrEmpty(stripeCustomerId))
		{
			throw new BusinessException("Stripe customer not found.");
		}
		PaymentMethod_db defaultPaymentMethod = await _paymentMethodRepository.GetDefaultPaymentMethodAsync(userId);
		if (defaultPaymentMethod == null)
		{
			return new BundlePurchaseResponse
			{
				success = true,
				bundle_order_id = null,
				message = "No default payment method found.",
				requires_payment_method = true
			};
		}
		BundleOrders order = new BundleOrders
		{
			user_id = userId,
			bundle_price_id = bundle_price_id,
			stripe_price_id = anyCredit.stripe_price_id,
			amount = anyCredit.price,
			currency = anyCredit.currency,
			status = "pending",
			created_at = DateTime.UtcNow
		};
		BundleOrders anyorder = await _ordersRepo.CreateAsync(order);
		_logger.LogInformation("Bundle order created. OrderId: {OrderId}", anyorder.id);
		long subtotalMinor = (long)Math.Round(anyCredit.price * 100m, MidpointRounding.AwayFromZero);
		UserAddress address = await _businessRepo.BusinessPrimaryAddressForUserId(userId);
		CustomerService _stripeCustomerService = new CustomerService(_stripeClient);
		await _stripeCustomerService.UpdateAsync(stripeCustomerId, new CustomerUpdateOptions
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
		CalculationCreateOptions taxCalculationOptions = new CalculationCreateOptions
		{
			Currency = anyCredit.currency,
			Customer = stripeCustomerId,
			LineItems = new List<CalculationLineItemOptions>
			{
				new CalculationLineItemOptions
				{
					Amount = subtotalMinor,
					Product = anyCredit.stripe_product_id,
					Quantity = 1L,
					Reference = anyorder.id.ToString(),
					TaxBehavior = "exclusive"
				}
			},
			Expand = new List<string> { "line_items" }
		};
		Calculation taxCalculation;
		try
		{
			CalculationService taxCalculationService = new CalculationService(_stripeClient);
			taxCalculation = await taxCalculationService.CreateAsync(taxCalculationOptions, null, cancellationToken);
			_logger.LogInformation("Stripe Tax calculation created. CalculationId: {CalculationId}, Subtotal: {Subtotal}, Total: {Total}", taxCalculation.Id, (decimal)subtotalMinor / 100m, (decimal)taxCalculation.AmountTotal / 100m);
		}
		catch (StripeException exception)
		{
			_logger.LogError(exception, "Stripe Tax calculation failed. OrderId: {OrderId}", anyorder.id);
			throw new BusinessException("Unable to calculate VAT. Please try again.");
		}
		long amountTotalMinor = taxCalculation.AmountTotal;
		decimal amountSubtotal = (decimal)subtotalMinor / 100m;
		decimal amountTotal = (decimal)amountTotalMinor / 100m;
		decimal vatAmount = amountTotal - amountSubtotal;
		_logger.LogInformation("Bundle VAT calculated. OrderId: {OrderId}, Subtotal: {Subtotal}, VAT: {VAT}, Total: {Total}", anyorder.id, amountSubtotal, vatAmount, amountTotal);
		Dictionary<string, string> metadata = new Dictionary<string, string>
		{
			{
				"user_id",
				userId.ToString()
			},
			{
				"bundle_order_id",
				anyorder.id.ToString()
			},
			{
				"plan_price_id",
				bundle_price_id.ToString()
			},
			{ "source_type", "credit_bundle" },
			{ "action", "creditbundle_created" },
			{
				"updated_by",
				_identityService.GetUserId().ToString()
			},
			{ "reason", "User purchased credit bundle (direct charge)" },
			{ "tax_calculation_id", taxCalculation.Id },
			{
				"amount_subtotal",
				amountSubtotal.ToString("0.00")
			},
			{
				"vat_amount",
				vatAmount.ToString("0.00")
			},
			{
				"amount_total",
				amountTotal.ToString("0.00")
			}
		};
		PaymentIntentCreateOptions paymentIntentOptions = new PaymentIntentCreateOptions
		{
			Amount = amountTotalMinor,
			Currency = anyCredit.currency,
			Customer = stripeCustomerId,
			PaymentMethod = defaultPaymentMethod.stripe_payment_method_id,
			Confirm = true,
			OffSession = true,
			Metadata = metadata
		};
		PaymentIntent paymentIntent;
		try
		{
			RequestOptions requestOptions = new RequestOptions
			{
				IdempotencyKey = anyorder.id.ToString()
			};
			paymentIntent = await _paymentIntentService.CreateAsync(paymentIntentOptions, requestOptions, cancellationToken);
			_logger.LogInformation("PaymentIntent created successfully. PaymentIntentId: {PaymentIntentId}, Status: {Status}, Amount: {Amount}", paymentIntent.Id, paymentIntent.Status, (decimal)paymentIntent.Amount / 100m);
		}
		catch (StripeException exception2)
		{
			_logger.LogError(exception2, "Stripe payment failed for OrderId {OrderId}", anyorder.id);
			throw new BusinessException("Payment failed. Please try again.");
		}
		return new BundlePurchaseResponse
		{
			success = true,
			bundle_order_id = order.id,
			message = ((paymentIntent.Status == "succeeded") ? "Payment completed successfully." : "Payment initiated successfully."),
			requires_action = (paymentIntent.Status == "requires_action"),
			client_secret = paymentIntent.ClientSecret
		};
	}

	public async Task CreditBundlePurchaseCompletedAsync(BundlePurchaseCompletedDto dto)
	{
		try
		{
			await _ordersRepo.CreditBundlePurchaseCompletedAsync(dto);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Failed to complete bundle checkout for order {bundle_order_id}", dto.bundle_order_id);
			throw;
		}
	}

	public async Task OnBundleOrderMarkFailedAsync(BundleCheckoutFailedDto dto)
	{
		try
		{
			await _ordersRepo.BundleOrderMarkFailedAsync(dto);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Failed to complete bundle checkout for order {bundle_order_id}", dto.bundle_order_id);
			throw;
		}
	}
    
}

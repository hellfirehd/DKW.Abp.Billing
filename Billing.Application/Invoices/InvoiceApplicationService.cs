using Billing.Payments;
using Billing.Refunds;

namespace Billing.Invoices;

/// <summary>
/// Application service for invoice operations
/// </summary>
public class InvoiceApplicationService : IInvoiceApplicationService
{
    private readonly Dictionary<Guid, Invoice> _invoices = [];
    private readonly Dictionary<Guid, Discount> _discounts = [];
    private readonly Dictionary<Guid, Surcharge> _surcharges = [];

    private readonly ITaxProvider _taxProvider = new FakeTaxProvider();

    /// <summary>
    /// Creates a new invoice
    /// </summary>
    public Task<Guid> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = request.InvoiceNumber,
            DueDate = request.DueDate,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            BillingAddress = request.BillingAddress,
            ShippingAddress = request.ShippingAddress,
            Province = request.State,
            ShippingCost = request.ShippingCost,
            ShippingMethod = request.ShippingMethod
        };

        // Add items
        foreach (var itemRequest in request.Items)
        {
            var item = CreateInvoiceItem(itemRequest);
            invoice.AddItem(item);
        }

        // Apply tax rates based on state
        var taxes = _taxProvider.GetTaxRates(request.State, invoice.InvoiceDate);
        invoice.SetTaxRates(taxes);

        _invoices[invoice.Id] = invoice;
        return Task.FromResult(invoice.Id);
    }

    /// <summary>
    /// Gets an invoice by ID
    /// </summary>
    public Task<InvoiceResponse?> GetInvoiceAsync(Guid invoiceId)
    {
        if (!_invoices.TryGetValue(invoiceId, out var invoice))
        {
            return Task.FromResult<InvoiceResponse?>(null);
        }

        var response = MapToInvoiceResponse(invoice);
        return Task.FromResult<InvoiceResponse?>(response);
    }

    public Task UpdateInvoiceAsync(Guid invoiceId, UpdateInvoiceRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adds an item to an invoice
    /// </summary>
    public Task AddInvoiceItemAsync(Guid invoiceId, CreateInvoiceItemRequest request)
    {
        if (!_invoices.TryGetValue(invoiceId, out var invoice))
        {
            throw new ArgumentException($"Invoice {invoiceId} not found");
        }

        var item = CreateInvoiceItem(request);
        invoice.AddItem(item);

        return Task.CompletedTask;
    }

    public Task RemoveInvoiceItemAsync(Guid invoiceId, string itemId)
    {
        throw new NotImplementedException();
    }

    public Task ApplyDiscountsAsync(Guid invoiceId, List<string> discountIds)
    {
        throw new NotImplementedException();
    }

    public Task ApplySurchargesAsync(Guid invoiceId, List<string> surchargeIds)
    {
        throw new NotImplementedException();
    }

    public Task SetTaxRatesAsync(Guid invoiceId)
    {
        throw new NotImplementedException();
    }

    public Task MarkInvoiceAsSentAsync(Guid invoiceId)
    {
        throw new NotImplementedException();
    }

    public Task CancelInvoiceAsync(Guid invoiceId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Processes a payment for an invoice
    /// </summary>
    public Task<Guid> ProcessPaymentAsync(CreatePaymentRequest request)
    {
        if (!_invoices.TryGetValue(request.InvoiceId, out var invoice))
        {
            throw new ArgumentException($"Invoice {request.InvoiceId} not found");
        }

        var payment = new Payment
        {
            Amount = request.Amount,
            Method = request.Method,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            GatewayTransactionId = request.GatewayTransactionId ?? string.Empty,
            GatewayName = request.GatewayName ?? string.Empty
        };

        // For demo purposes, mark as completed immediately
        payment.MarkAsCompleted(request.GatewayTransactionId);

        invoice.AddPayment(payment);

        return Task.FromResult(payment.Id);
    }

    /// <summary>
    /// Processes a refund for an invoice
    /// </summary>
    public Task<Guid> ProcessRefundAsync(CreateRefundRequest request)
    {
        if (!_invoices.TryGetValue(request.InvoiceId, out var invoice))
        {
            throw new ArgumentException($"Invoice {request.InvoiceId} not found");
        }

        var refund = Refund.CreateProportionalRefund(
            invoice,
            request.Amount,
            request.Reason,
            request.OriginalPaymentId);

        if (!string.IsNullOrEmpty(request.ReferenceNumber))
        {
            refund.ReferenceNumber = request.ReferenceNumber;
        }

        if (!string.IsNullOrEmpty(request.Notes))
        {
            refund.Notes = request.Notes;
        }

        invoice.ProcessRefund(refund);

        return Task.FromResult(refund.Id);
    }

    /// <summary>
    /// Adds a discount definition
    /// </summary>
    public void AddDiscount(Discount discount)
    {
        _discounts[discount.Id] = discount;
    }

    /// <summary>
    /// Adds a surcharge definition
    /// </summary>
    public void AddSurcharge(Surcharge surcharge)
    {
        _surcharges[surcharge.Id] = surcharge;
    }

    public Task<List<InvoiceResponse>> GetInvoicesAsync(InvoiceFilter? filter = null)
    {
        throw new NotImplementedException();
    }

    public Task MoveItemUp(Guid invoiceId, string itemId)
    {
        throw new NotImplementedException();
    }

    public Task MoveItemDown(Guid invoiceId, string itemId)
    {
        throw new NotImplementedException();
    }

    private InvoiceItem CreateInvoiceItem(CreateInvoiceItemRequest request)
    {
        InvoiceItem item = request.ItemType.ToLower() switch
        {
            "service" => new Service
            {
                Description = request.Description,
                HourlyRate = request.HourlyRate ?? 0,
                Hours = request.Hours ?? 1,
                TaxCategory = request.Category
            },
            "product" or _ => new Product
            {
                Description = request.Description,
                UnitPrice = request.UnitPrice,
                Quantity = request.Quantity,
                TaxCategory = request.Category,
                SKU = request.SKU ?? string.Empty,
                Weight = request.Weight ?? 0,
                Manufacturer = request.Manufacturer ?? string.Empty,
                RequiresShipping = request.RequiresShipping
            }
        };

        return item;
    }

    private InvoiceResponse MapToInvoiceResponse(Invoice invoice)
    {
        return new InvoiceResponse
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status,
            CustomerName = invoice.CustomerName,
            CustomerEmail = invoice.CustomerEmail,
            BillingAddress = invoice.BillingAddress,
            ShippingAddress = invoice.ShippingAddress,
            State = invoice.Province,
            Subtotal = invoice.GetSubtotal(),
            TotalTax = invoice.GetTotalTax(),
            TotalDiscounts = invoice.GetLineItemDiscounts() + invoice.GetOrderDiscounts(),
            TotalSurcharges = invoice.GetTotalSurcharges(),
            ShippingCost = invoice.ShippingCost,
            Total = invoice.GetTotal(),
            TotalPaid = invoice.GetTotalPaid(),
            Balance = invoice.GetBalance(),
            Items = invoice.Items.Select(MapToInvoiceItemResponse).ToList(),
            Payments = invoice.Payments.Select(MapToPaymentResponse).ToList(),
            Refunds = invoice.Refunds.Select(MapToRefundResponse).ToList()
        };
    }

    private InvoiceItemResponse MapToInvoiceItemResponse(InvoiceItem item)
    {
        return new InvoiceItemResponse
        {
            Id = item.Id,
            Description = item.Description,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            Subtotal = item.GetSubtotal(),
            TotalTax = item.GetTotalTax(),
            TotalDiscount = item.GetTotalDiscount(),
            Total = item.GetTotal(),
            Category = item.TaxCategory,
            ItemType = item.GetType().Name
        };
    }

    private PaymentResponse MapToPaymentResponse(Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Method = payment.Method,
            Status = payment.Status,
            PaymentDate = payment.PaymentDate,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes
        };
    }

    private RefundResponse MapToRefundResponse(Refund refund)
    {
        return new RefundResponse
        {
            Id = refund.Id,
            Amount = refund.Amount,
            RefundDate = refund.RefundDate,
            Reason = refund.Reason,
            ReferenceNumber = refund.ReferenceNumber,
            OriginalPaymentId = refund.OriginalPaymentId
        };
    }
}

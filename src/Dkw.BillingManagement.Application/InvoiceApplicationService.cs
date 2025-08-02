namespace Dkw.BillingManagement;

/// <summary>
/// Application service for invoice operations
/// </summary>
public class InvoiceApplicationService(InvoiceItemManager invoiceItemManager, ITaxProvider taxProvider) : IInvoiceApplicationService
{
    // ToDo: Inject repositories for persistence
    private readonly Dictionary<Guid, Invoice> _invoices = [];
    private readonly Dictionary<Guid, Item> _items = [];
    private readonly Dictionary<Guid, Discount> _discounts = [];
    private readonly Dictionary<Guid, Surcharge> _surcharges = [];

    private readonly InvoiceItemManager _invoiceItemManager = invoiceItemManager;
    private readonly ITaxProvider _taxProvider = taxProvider;

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
            Province = request.State,
            ShippingCost = request.ShippingCost,
            ShippingMethod = request.ShippingMethod
        };

        // Add items
        foreach (var itemRequest in request.Items)
        {
            var item = CreateInvoiceItem(invoice, itemRequest);
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

        var item = CreateInvoiceItem(invoice, request);
        invoice.AddItem(item);

        return Task.CompletedTask;
    }

    public Task RemoveInvoiceItemAsync(Guid invoiceId, String itemId)
    {
        throw new NotImplementedException();
    }

    public Task ApplyDiscountsAsync(Guid invoiceId, List<String> discountIds)
    {
        throw new NotImplementedException();
    }

    public Task ApplySurchargesAsync(Guid invoiceId, List<String> surchargeIds)
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
            GatewayTransactionId = request.GatewayTransactionId ?? String.Empty,
            GatewayName = request.GatewayName ?? String.Empty
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

        if (!String.IsNullOrEmpty(request.ReferenceNumber))
        {
            refund.ReferenceNumber = request.ReferenceNumber;
        }

        if (!String.IsNullOrEmpty(request.Notes))
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

    public Task MoveItemUp(Guid invoiceId, String itemId)
    {
        throw new NotImplementedException();
    }

    public Task MoveItemDown(Guid invoiceId, String itemId)
    {
        throw new NotImplementedException();
    }

    private InvoiceItem CreateInvoiceItem(Invoice invoice, CreateInvoiceItemRequest request)
    {
        return _invoiceItemManager.Create(request.ItemId, invoice.InvoiceDate, request.Quantity);
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
            TotalTax = item.GetTaxTotal(),
            TotalDiscount = item.GetDiscountTotal(),
            Total = item.GetTotal(),
            ItemType = item.ItemType
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

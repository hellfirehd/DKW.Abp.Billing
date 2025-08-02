namespace Dkw.Abp.Billing;

public class InvoiceResponse
{
    public Guid Id { get; set; }
    public String InvoiceNumber { get; set; } = String.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public String CustomerName { get; set; } = String.Empty;
    public String CustomerEmail { get; set; } = String.Empty;
    public String BillingAddress { get; set; } = String.Empty;
    public String ShippingAddress { get; set; } = String.Empty;
    public String State { get; set; } = String.Empty;
    public Decimal Subtotal { get; set; }
    public Decimal TotalTax { get; set; }
    public Decimal TotalDiscounts { get; set; }
    public Decimal TotalSurcharges { get; set; }
    public Decimal ShippingCost { get; set; }
    public Decimal Total { get; set; }
    public Decimal TotalPaid { get; set; }
    public Decimal TotalRefunded { get; set; }
    public Decimal Balance { get; set; }
    public List<InvoiceItemResponse> Items { get; set; } = [];
    public List<PaymentResponse> Payments { get; set; } = [];
    public List<RefundResponse> Refunds { get; set; } = [];
}

namespace Billing;

public class InvoiceResponse
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal TotalSurcharges { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalRefunded { get; set; }
    public decimal Balance { get; set; }
    public List<InvoiceItemResponse> Items { get; set; } = [];
    public List<PaymentResponse> Payments { get; set; } = [];
    public List<RefundResponse> Refunds { get; set; } = [];
}

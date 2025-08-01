namespace Billing;

// Request/Response DTOs
public class CreateInvoiceRequest
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly? DueDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public Province State { get; set; } = Province.Empty;
    public List<CreateInvoiceItemRequest> Items { get; set; } = [];
    public decimal ShippingCost { get; set; }
    public string ShippingMethod { get; set; } = string.Empty;
}

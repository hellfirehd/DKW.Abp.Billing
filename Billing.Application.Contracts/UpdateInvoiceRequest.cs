namespace Billing;

public class UpdateInvoiceRequest
{
    public string? InvoiceNumber { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? BillingAddress { get; set; }
    public string? ShippingAddress { get; set; }
    public string? State { get; set; }
    public decimal? ShippingCost { get; set; }
    public string? ShippingMethod { get; set; }
}

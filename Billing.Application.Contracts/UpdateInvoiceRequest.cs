namespace Billing;

public class UpdateInvoiceRequest
{
    public String? InvoiceNumber { get; set; }
    public DateOnly? DueDate { get; set; }
    public String? CustomerName { get; set; }
    public String? CustomerEmail { get; set; }
    public String? BillingAddress { get; set; }
    public String? ShippingAddress { get; set; }
    public String? State { get; set; }
    public Decimal? ShippingCost { get; set; }
    public String? ShippingMethod { get; set; }
}

namespace Dkw.BillingManagement;

// Request/Response DTOs
public class CreateInvoiceRequest
{
    public String InvoiceNumber { get; set; } = String.Empty;
    public DateOnly? DueDate { get; set; }
    public String CustomerName { get; set; } = String.Empty;
    public String CustomerEmail { get; set; } = String.Empty;
    public String BillingManagementAddress { get; set; } = String.Empty;
    public String ShippingAddress { get; set; } = String.Empty;
    public Province State { get; set; } = Province.Empty;
    public List<CreateInvoiceItemRequest> Items { get; set; } = [];
    public Decimal ShippingCost { get; set; }
    public String ShippingMethod { get; set; } = String.Empty;
}

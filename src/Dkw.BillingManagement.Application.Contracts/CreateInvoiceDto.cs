namespace Dkw.BillingManagement;

/// <summary>
/// DTO for creating a new invoice
/// </summary>
public class CreateInvoiceDto
{
    public String InvoiceNumber { get; set; } = String.Empty;
    public DateOnly? DueDate { get; set; }
    public String CustomerName { get; set; } = String.Empty;
    public String CustomerEmail { get; set; } = String.Empty;
    public String BillingManagementAddress { get; set; } = String.Empty;
    public String ShippingAddress { get; set; } = String.Empty;
    public String State { get; set; } = String.Empty;
    public List<CreateInvoiceItemDto> Items { get; set; } = [];
    public Decimal ShippingCost { get; set; }
    public String ShippingMethod { get; set; } = String.Empty;
}

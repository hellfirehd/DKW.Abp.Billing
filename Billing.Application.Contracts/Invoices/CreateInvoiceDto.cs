namespace Billing.Invoices;

/// <summary>
/// DTO for creating a new invoice
/// </summary>
public class CreateInvoiceDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly? DueDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public List<CreateInvoiceItemDto> Items { get; set; } = [];
    public decimal ShippingCost { get; set; }
    public string ShippingMethod { get; set; } = string.Empty;
}

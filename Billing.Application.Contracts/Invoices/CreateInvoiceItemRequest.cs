namespace Billing.Invoices;

public class CreateInvoiceItemRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public TaxCategory Category { get; set; }
    public string ItemType { get; set; } = "Product"; // "Product" or "Service"

    // Product specific fields
    public string? SKU { get; set; }
    public decimal? Weight { get; set; }
    public string? Manufacturer { get; set; }
    public bool RequiresShipping { get; set; } = true;

    // Service specific fields
    public decimal? HourlyRate { get; set; }
    public decimal? Hours { get; set; }
}

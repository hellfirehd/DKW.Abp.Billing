namespace Billing;

/// <summary>
/// DTO for creating invoice items
/// </summary>
public class CreateInvoiceItemDto
{
    public String Description { get; set; } = String.Empty;
    public Decimal UnitPrice { get; set; }
    public Int32 Quantity { get; set; } = 1;
    public TaxCategory Category { get; set; }
    public String ItemType { get; set; } = "Product"; // "Product" or "Service"

    // Product specific fields
    public String? SKU { get; set; }
    public Decimal? Weight { get; set; }
    public String? Manufacturer { get; set; }
    public bool RequiresShipping { get; set; } = true;

    // Service specific fields
    public Decimal? HourlyRate { get; set; }
    public Decimal? Hours { get; set; }
}

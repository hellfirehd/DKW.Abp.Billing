namespace Billing;

/// <summary>
/// Represents a physical product that can be sold
/// </summary>
public class Product : InvoiceItem
{
    public string SKU { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public bool RequiresShipping { get; set; } = true;
}

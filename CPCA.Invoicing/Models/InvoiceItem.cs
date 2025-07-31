namespace CPCA.Invoicing.Models;

// Base models for invoice items
public abstract class InvoiceItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public virtual decimal GetSubtotal() => UnitPrice * Quantity;
    public List<Tax> ApplicableTaxes { get; set; } = new List<Tax>();
    public List<Discount> Discounts { get; set; } = new List<Discount>();
}

public class Product : InvoiceItem
{
    public string SKU { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
}

public class Service : InvoiceItem
{
    public decimal HourlyRate { get; set; }
    public decimal Hours { get; set; }
    
    public override decimal GetSubtotal() => HourlyRate * Hours;
}
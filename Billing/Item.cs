namespace Billing;

public abstract class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string UnitType { get; set; } = "Each";
    public ItemClassification Classification { get; set; } = default!;
    public TaxCategory TaxCategory { get; set; } = TaxCategory.DigitalProduct;

    public bool IsActive { get; set; } = true;
}

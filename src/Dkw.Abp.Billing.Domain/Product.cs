namespace Dkw.Abp.Billing;

/// <summary>
/// Represents a physical product that can be sold
/// </summary>
public class Product : Item<Product>
{
    public static Product Create(Guid id, String sku, String name, ItemCategory itemCategory)
    {
        return new Product
        {
            Id = id,
            SKU = sku,
            Name = name,
            ItemCategory = itemCategory
        };
    }

    public override ItemType ItemType => ItemType.Product;
    public Decimal Weight { get; set; }
    public String Manufacturer { get; set; } = String.Empty;
    public Boolean RequiresShipping { get; set; } = true;
}

public class ProductInvoiceItemFactory : InvoiceItemFactory<Product>
{
    protected override void SetProperties(Product item, ref InvoiceItem invoiceItem)
    {
    }
}
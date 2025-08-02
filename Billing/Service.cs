namespace Billing;

/// <summary>
/// Represents a service that can be sold
/// </summary>
public class Service : Item<Service>
{
    public static Service Create(Guid id, String serviceType, String providerName, ItemCategory itemCategory)
    {
        return new Service()
        {
            Id = id,
            ServiceType = serviceType,
            ProviderName = providerName,
            ItemCategory = itemCategory
        };
    }

    public Service()
    {
        UnitType = "Hour";
    }

    public override ItemType ItemType => ItemType.Service;
    public String ServiceType { get; set; } = String.Empty;
    public String ProviderName { get; set; } = String.Empty;
    public DateOnly? ServiceDate { get; set; }
    public TimeSpan? Duration { get; set; }
}

public class ServiceInvoiceItemFactory : InvoiceItemFactory<Service>
{
    protected override void SetProperties(Service item, ref InvoiceItem invoiceItem)
    {
    }
}
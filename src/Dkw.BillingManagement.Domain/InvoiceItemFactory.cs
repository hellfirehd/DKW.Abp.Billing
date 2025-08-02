namespace Dkw.BillingManagement;

public abstract class InvoiceItemFactory<T> : IInvoiceItemFactory
    where T : Item<T>
{
    public Boolean CanCreate(Item item) => item is T;

    public InvoiceItem Create(Item item, DateOnly invoiceDate)
    {
        if (!CanCreate(item))
        {
            throw new ArgumentException($"{nameof(Item.ItemType)} must be of type {ItemType.Product}.", nameof(item));
        }

        if (!item.IsAvailableOn(invoiceDate))
        {
            throw new ArgumentOutOfRangeException(nameof(invoiceDate),
                "Effective date must be on or after the earliest price effective date.");
        }

        var invoiceItem = new InvoiceItem
        {
            InvoiceDate = invoiceDate,
            SortOrder = 0, // Default sort order, can be adjusted later
            Id = Guid.NewGuid(), // ToDo: Replace with a more robust ID generation strategy
            ItemId = item.Id,
            SKU = item.SKU,
            Name = item.Name,
            Description = item.Description,
            UnitType = item.UnitType,
            ItemType = item.ItemType,
            ItemCategory = item.ItemCategory,
            TaxCode = item.TaxCode,
            IsActive = item.IsActive,
            UnitPrice = item.GetUnitPrice(invoiceDate)
        };

        SetProperties((T)item, ref invoiceItem);

        return invoiceItem;
    }

    protected virtual void SetProperties(T item, ref InvoiceItem invoiceItem) { }
}

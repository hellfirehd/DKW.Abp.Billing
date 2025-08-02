namespace Dkw.Abp.Billing;

/// <summary>
/// Represents an abstract item with properties for identification, classification, pricing, and tax information.
/// </summary>
/// <remarks>
/// This class serves as a base type for items in an inventory or catalog system. It includes essential
/// properties  such as ID, name, description, classification, tax code, unit price, and unit type. The <see
/// cref="IsActive"/>  property indicates whether the item is currently active or available.
/// </remarks>
public abstract class Item
{
    protected List<ItemPrice> Pricing { get; } = [];

    protected Item()
    {
        // Parameterless constructor for EF Core/JSON serialization
    }

    public Item(
        Guid id,
        String code,
        String name,
        String description,
        ItemType itemType,
        ItemCategory itemCategory,
        TaxCode taxCode)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentNullException(nameof(id), "Id cannot be empty.")
            : id;
        SKU = code;
        Name = name
            ?? throw new ArgumentNullException(nameof(name), "Name cannot be null.");

        Description = description
            ?? throw new ArgumentNullException(nameof(description), "Description cannot be null.");

        ItemCategory = itemCategory;

        TaxCode = taxCode
            ?? throw new ArgumentNullException(nameof(taxCode), "Tax Code cannot be null.");

        UnitType = "Each";

        IsActive = true;
    }

    public Guid Id { get; set; }
    public String SKU { get; set; } = String.Empty;
    public String Name { get; set; } = String.Empty;
    public String Description { get; set; } = String.Empty;
    public virtual String UnitType { get; set; } = "Each";
    public abstract ItemType ItemType { get; }
    public ItemCategory ItemCategory { get; set; } = ItemCategory.GeneralGoods;
    public TaxCode TaxCode { get; set; } = TaxCode.Empty;
    public Boolean IsActive { get; set; } = true;

    public Boolean IsAvailableOn(DateOnly date)
    {
        if (Pricing is null || Pricing.Count == 0)
        {
            return false; // No pricing information available
        }

        return Pricing.Any(p => p.InEffect(date));
    }

    public Decimal GetUnitPrice(DateOnly effectiveDate)
    {
        if (!IsAvailableOn(effectiveDate))
        {
            throw new InvalidOperationException("This item has not been priced.");
        }

        // Find the most recent price that is effective on or before the given date
        return Pricing
            .Where(p => p.InEffect(effectiveDate))
            .OrderByDescending(p => p.EffectiveDate)
            .Select(p => p.UnitPrice)
            .FirstOrDefault();
    }

    public Boolean IsValid()
    {
        return !String.IsNullOrWhiteSpace(SKU)
            && !String.IsNullOrWhiteSpace(Name)
            && Description != null
            && ItemType != ItemType.None
            && ItemCategory != ItemCategory.None
            && TaxCode != null
            && Pricing.Count > 0;
    }
}

public abstract class Item<T> : Item
    where T : Item<T>
{
    public T AddPrice(Decimal unitPrice, DateOnly effectiveDate, DateOnly? expirationDate = null)
    {
        if (unitPrice < Decimal.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice),
                "Unit Price cannot be less than $0.00. If you need a negative amount, "
                + "apply a discount to the Invoice Line Item.");
        }

        Pricing.Add(new ItemPrice(unitPrice, effectiveDate, expirationDate));

        return (T)this;
    }
}
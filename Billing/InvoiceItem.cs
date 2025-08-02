namespace Billing;

/// <summary>
/// Base class for all invoice items (products and services)
/// </summary>
public class InvoiceItem
{

    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the original item.
    /// </summary>
    public required Guid ItemId { get; set; }
    public required String SKU { get; set; } = String.Empty;
    public required String Name { get; set; } = String.Empty;
    public String Description { get; set; } = String.Empty;
    public String UnitType { get; set; } = "Each";
    public ItemType ItemType { get; set; }
    public ItemCategory ItemCategory { get; set; } = ItemCategory.GeneralGoods;
    public TaxCode TaxCode { get; set; } = TaxCode.Empty;
    public Boolean IsActive { get; set; } = true;

    public ItemClassification ItemClassification { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public Decimal Quantity { get; set; } = 1.0m;
    public List<TaxAmount> AppliedTaxes { get; private set; } = [];
    public List<Discount> AppliedDiscounts { get; private set; } = [];
    public Int32 SortOrder { get; set; }

    public Decimal UnitPrice { get; set; }

    public InvoiceItem ChangeQuantity(Decimal newQuantity)
    {
        if (newQuantity < Decimal.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity must be greater than zero.");
        }

        Quantity = newQuantity;

        return this;
    }

    /// <summary>
    /// Applies taxes to this line item
    /// </summary>
    public void ApplyTaxes(IEnumerable<TaxRate> taxRates)
    {
        AppliedTaxes.Clear();
        var taxableAmount = GetDiscountedTotal();

        foreach (var taxRate in taxRates)
        {
            // How do we determine if a tax rate applies?
            var taxAmount = new TaxAmount
            {
                TaxRate = taxRate,
                TaxableAmount = taxableAmount,
                Amount = Math.Round(taxableAmount * taxRate.Rate, 2)
            };
            AppliedTaxes.Add(taxAmount);
        }
    }

    /// <summary>
    /// Applies discounts to this line item
    /// </summary>
    public void ApplyDiscounts(IEnumerable<Discount> discounts)
    {
        AppliedDiscounts.Clear();
        AppliedDiscounts.AddRange(discounts.Where(d =>
            d.Scope == DiscountScope.PerItem && d.IsActive));
    }

    /// <summary>
    /// Gets the subtotal before taxes and discounts
    /// </summary>
    public virtual Decimal GetSubtotal() => UnitPrice * Quantity;

    /// <summary>
    /// Gets the total after applying discounts but before taxes
    /// </summary>
    public virtual Decimal GetDiscountedTotal()
    {
        var subtotal = GetSubtotal();
        var itemDiscounts = AppliedDiscounts.Where(d => d.Scope == DiscountScope.PerItem);

        foreach (var discount in itemDiscounts)
        {
            subtotal -= discount.CalculateDiscountAmount(subtotal);
        }

        return Math.Max(0, subtotal);
    }

    /// <summary>
    /// Gets the total tax amount for this line item
    /// </summary>
    public Decimal GetTaxTotal() => AppliedTaxes.Sum(t => t.Amount);

    /// <summary>
    /// Gets the total discount amount for this line item
    /// </summary>
    public Decimal GetDiscountTotal()
    {
        var subtotal = GetSubtotal();
        return AppliedDiscounts
            .Where(d => d.Scope == DiscountScope.PerItem)
            .Sum(d => d.CalculateDiscountAmount(subtotal));
    }

    /// <summary>
    /// Gets the final total including taxes
    /// </summary>
    public Decimal GetTotal() => GetDiscountedTotal() + GetTaxTotal();
}

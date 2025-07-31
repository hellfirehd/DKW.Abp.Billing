namespace Billing;

/// <summary>
/// Base class for all invoice items (products and services)
/// </summary>
public abstract class InvoiceItem : Item
{
    public int Quantity { get; set; } = 1;
    public List<TaxAmount> AppliedTaxes { get; private set; } = [];
    public List<Discount> AppliedDiscounts { get; private set; } = [];
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets the subtotal before taxes and discounts
    /// </summary>
    public virtual decimal GetSubtotal() => UnitPrice * Quantity;

    /// <summary>
    /// Gets the total after applying discounts but before taxes
    /// </summary>
    public virtual decimal GetDiscountedTotal()
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
    /// Applies taxes to this line item
    /// </summary>
    public void ApplyTaxes(IEnumerable<ItemTaxRate> taxes)
    {
        AppliedTaxes.Clear();
        var taxableAmount = GetDiscountedTotal();

        foreach (var tax in taxes)
        {
            if (tax.TaxCategory == TaxCategory)
            {
                var taxAmount = new TaxAmount
                {
                    TaxRate = tax.TaxRate,
                    TaxableAmount = taxableAmount,
                    Amount = Math.Round(taxableAmount * tax.TaxRate.Rate, 2)
                };
                AppliedTaxes.Add(taxAmount);
            }
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
    /// Gets the total tax amount for this line item
    /// </summary>
    public decimal GetTotalTax() => AppliedTaxes.Sum(t => t.Amount);

    /// <summary>
    /// Gets the total discount amount for this line item
    /// </summary>
    public decimal GetTotalDiscount()
    {
        var subtotal = GetSubtotal();
        return AppliedDiscounts
            .Where(d => d.Scope == DiscountScope.PerItem)
            .Sum(d => d.CalculateDiscountAmount(subtotal));
    }

    /// <summary>
    /// Gets the final total including taxes
    /// </summary>
    public decimal GetTotal() => GetDiscountedTotal() + GetTotalTax();
}

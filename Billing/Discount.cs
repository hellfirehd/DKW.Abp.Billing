namespace Billing;

/// <summary>
/// Represents a discount that can be applied to items or orders
/// </summary>
public class Discount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public String Name { get; set; } = String.Empty;
    public String Description { get; set; } = String.Empty;
    public DiscountType Type { get; set; }
    public DiscountScope Scope { get; set; }
    public Decimal Value { get; set; } // Percentage (0-1) or fixed amount
    public Decimal? MinimumAmount { get; set; } // Minimum order amount for discount to apply
    public Decimal? MaximumDiscount { get; set; } // Maximum discount amount
    public bool IsActive { get; set; } = true;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Calculates the discount amount based on the base amount
    /// </summary>
    public Decimal CalculateDiscountAmount(Decimal baseAmount)
    {
        if (!IsActive || StartDate.HasValue && DateOnly.FromDateTime(DateTime.UtcNow) < StartDate.Value ||
            EndDate.HasValue && DateOnly.FromDateTime(DateTime.UtcNow) > EndDate.Value)
        {
            return 0;
        }

        if (MinimumAmount.HasValue && baseAmount < MinimumAmount.Value)
        {
            return 0;
        }

        Decimal discountAmount = Type == DiscountType.Percentage
            ? baseAmount * Value
            : Value;

        if (MaximumDiscount.HasValue && discountAmount > MaximumDiscount.Value)
        {
            discountAmount = MaximumDiscount.Value;
        }

        return Math.Round(discountAmount, 2);
    }
}

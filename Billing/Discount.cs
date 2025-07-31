namespace Billing;

/// <summary>
/// Represents a discount that can be applied to items or orders
/// </summary>
public class Discount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DiscountType Type { get; set; }
    public DiscountScope Scope { get; set; }
    public decimal Value { get; set; } // Percentage (0-1) or fixed amount
    public decimal? MinimumAmount { get; set; } // Minimum order amount for discount to apply
    public decimal? MaximumDiscount { get; set; } // Maximum discount amount
    public bool IsActive { get; set; } = true;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Calculates the discount amount based on the base amount
    /// </summary>
    public decimal CalculateDiscountAmount(decimal baseAmount)
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

        decimal discountAmount = Type == DiscountType.Percentage
            ? baseAmount * Value
            : Value;

        if (MaximumDiscount.HasValue && discountAmount > MaximumDiscount.Value)
        {
            discountAmount = MaximumDiscount.Value;
        }

        return Math.Round(discountAmount, 2);
    }
}

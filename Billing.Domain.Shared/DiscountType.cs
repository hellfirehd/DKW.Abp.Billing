using System.ComponentModel;

namespace Billing;

/// <summary>
/// Represents different types of discounts
/// </summary>
public enum DiscountType
{
    [Description("Percentage")]
    Percentage,

    [Description("Fixed Amount")]
    FixedAmount,

    [Description("Percentage and Fixed Amount")]
    Both
}

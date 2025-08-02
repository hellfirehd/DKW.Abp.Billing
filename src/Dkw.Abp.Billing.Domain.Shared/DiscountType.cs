using System.ComponentModel;

namespace Dkw.Abp.Billing;

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

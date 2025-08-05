using System.ComponentModel;

namespace Dkw.BillingManagement.Invoices;

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

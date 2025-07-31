using System.ComponentModel;

namespace Billing;

/// <summary>
/// Represents different discount scopes
/// </summary>
public enum DiscountScope
{
    [Description("Per Item")]
    PerItem,

    [Description("Per Order")]
    PerOrder
}

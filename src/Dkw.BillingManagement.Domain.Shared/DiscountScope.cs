using System.ComponentModel;

namespace Dkw.BillingManagement;

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

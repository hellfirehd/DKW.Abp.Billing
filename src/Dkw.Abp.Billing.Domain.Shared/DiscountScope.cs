using System.ComponentModel;

namespace Dkw.Abp.Billing;

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

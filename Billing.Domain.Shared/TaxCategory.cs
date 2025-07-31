using System.ComponentModel;

namespace Billing;

/// <summary>
/// Represents product or service categories for tax purposes
/// </summary>
public enum TaxCategory
{
    [Description("Taxable Product")]
    TaxableProduct,

    [Description("Non-Taxable Product")]
    NonTaxableProduct,

    [Description("Taxable Service")]
    TaxableService,

    [Description("Non-Taxable Service")]
    NonTaxableService,

    [Description("Digital Product")]
    DigitalProduct,

    [Description("Shipping")]
    Shipping
}

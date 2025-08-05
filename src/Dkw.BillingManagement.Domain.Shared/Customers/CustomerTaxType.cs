using System.ComponentModel;

namespace Dkw.BillingManagement.Customers;

/// <summary>
/// Recipient status affecting tax treatment
/// </summary>
public enum CustomerTaxType
{
    [Description("Regular Consumer")]
    Regular = 0,

    [Description("GST/HST Registrant")]
    Registrant = 1,

    [Description("Tax-Exempt Organization")]
    TaxExempt = 2,

    [Description("First Nations")]
    FirstNations = 3,

    [Description("Non-Resident")]
    NonResident = 4,

    [Description("Government Entity")]
    Government = 5
}

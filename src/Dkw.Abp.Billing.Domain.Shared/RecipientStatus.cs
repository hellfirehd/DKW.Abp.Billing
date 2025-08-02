using System.ComponentModel;

namespace Dkw.Abp.Billing;

/// <summary>
/// Recipient status affecting tax treatment
/// </summary>
public enum RecipientStatus
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

using System.ComponentModel;

namespace Billing;

/// <summary>
/// Canadian GST/HST tax treatment classifications
/// Based on CRA guidelines for goods and services taxation
/// </summary>
public enum TaxTreatment
{
    /// <summary>
    /// Standard rate applies (GST/HST at full rate)
    /// Most goods and services fall into this category
    /// </summary>
    [Description("Taxable at Standard Rate")]
    Standard = 0,

    /// <summary>
    /// Zero-rated supplies (0% GST/HST but input tax credits available)
    /// Examples: Basic groceries, prescription drugs, medical devices, exports
    /// </summary>
    [Description("Zero-Rated")]
    ZeroRated = 1,

    /// <summary>
    /// GST/HST exempt (no GST/HST charged, no input tax credits)
    /// Examples: Healthcare services, educational services, financial services
    /// </summary>
    [Description("Exempt")]
    Exempt = 2,

    /// <summary>
    /// Not subject to GST/HST (outside the tax system)
    /// Examples: Employee wages, gifts, used goods sold by individuals
    /// </summary>
    [Description("Out of Scope")]
    OutOfScope = 3
}

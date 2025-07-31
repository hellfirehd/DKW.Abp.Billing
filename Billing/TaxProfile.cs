namespace Billing;

/// <summary>
/// Temporary alias for backward compatibility
/// Will be removed once CustomerTaxProfile is fully implemented
/// </summary>
[Obsolete("Use CustomerTaxProfile instead")]
public class TaxProfile
{
    public RecipientStatus RecipientStatus { get; set; } = RecipientStatus.Regular;
    public string GstHstNumber { get; set; } = string.Empty;
    public string ExemptionCertificateNumber { get; set; } = string.Empty;
}
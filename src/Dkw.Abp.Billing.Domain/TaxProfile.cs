namespace Dkw.Abp.Billing;

/// <summary>
/// Temporary alias for backward compatibility
/// Will be removed once CustomerTaxProfile is fully implemented
/// </summary>
[Obsolete("Use CustomerTaxProfile instead")]
public class TaxProfile
{
    public RecipientStatus RecipientStatus { get; set; } = RecipientStatus.Regular;
    public String GstHstNumber { get; set; } = String.Empty;
    public String ExemptionCertificateNumber { get; set; } = String.Empty;
}
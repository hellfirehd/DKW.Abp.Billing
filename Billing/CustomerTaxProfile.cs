namespace Billing;

/// <summary>
/// Customer tax status and exemption information
/// </summary>
public class CustomerTaxProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Customer identifier
    /// </summary>
    public String CustomerId { get; set; } = String.Empty;

    /// <summary>
    /// Customer's tax status
    /// </summary>
    public RecipientStatus RecipientStatus { get; set; } = RecipientStatus.Regular;

    /// <summary>
    /// GST/HST registration number (if applicable)
    /// </summary>
    public String GstHstNumber { get; set; } = String.Empty;

    /// <summary>
    /// Tax exemption certificate number (if applicable)
    /// </summary>
    public String ExemptionCertificateNumber { get; set; } = String.Empty;

    /// <summary>
    /// Province where customer is located for tax purposes
    /// </summary>
    public Province TaxProvince { get; set; } = Province.Empty;

    /// <summary>
    /// Whether customer is eligible for tax exemptions
    /// </summary>
    public bool IsEligibleForExemption { get; set; }

    /// <summary>
    /// Date when tax profile becomes effective
    /// </summary>
    public DateOnly EffectiveDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Date when tax profile expires
    /// </summary>
    public DateOnly? ExpirationDate { get; set; }

    /// <summary>
    /// Additional notes about customer's tax status
    /// </summary>
    public String Notes { get; set; } = String.Empty;

    /// <summary>
    /// Checks if customer qualifies for tax exemption on a specific date
    /// </summary>
    public virtual bool QualifiesForExemption(DateOnly date)
    {
        return IsEligibleForExemption
            && date >= EffectiveDate
            && (ExpirationDate == null || date <= ExpirationDate.Value);
    }
}

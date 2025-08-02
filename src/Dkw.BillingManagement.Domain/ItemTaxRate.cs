namespace Dkw.BillingManagement;

/// <summary>
/// Represents a tax rate for a specific item category
/// </summary>
[Obsolete("Use ItemClassification instead")]
public record ItemTaxRate(String Code, TaxCode TaxCode, TaxRate TaxRate);

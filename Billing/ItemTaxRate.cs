namespace Billing;

/// <summary>
/// Represents a tax rate for a specific item category
/// </summary>
public record ItemTaxRate(String Code, TaxType TaxType, TaxRate TaxRate);

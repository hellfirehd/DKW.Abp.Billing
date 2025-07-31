namespace Billing;

/// <summary>
/// Represents a tax rate for a specific item category
/// </summary>
public record ItemTaxRate(string Code, TaxCategory TaxCategory, TaxRate TaxRate);

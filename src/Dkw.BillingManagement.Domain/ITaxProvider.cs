namespace Dkw.BillingManagement;

/// <summary>
/// Interface for tax calculation providers
/// </summary>
public interface ITaxProvider
{
    /// <summary>
    /// Gets all applicable tax rates for a province on a specific date
    /// </summary>
    IEnumerable<TaxRate> GetTaxRates(Province province, DateOnly? effectiveDate = null);
}

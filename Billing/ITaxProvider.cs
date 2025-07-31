namespace Billing;

/// <summary>
/// Interface for tax calculation providers
/// </summary>
public interface ITaxProvider
{
    /// <summary>
    /// Gets all applicable tax rates for a province on a specific date
    /// </summary>
    ItemTaxRate[] GetTaxRates(Province province, DateOnly effectiveDate);

    /// <summary>
    /// Gets tax rates filtered by tax category for more precise tax calculation
    /// </summary>
    ItemTaxRate[] GetTaxRatesForCategory(Province province, DateOnly effectiveDate, TaxCategory taxCategory);

    /// <summary>
    /// Validates if a province has tax configuration
    /// </summary>
    bool HasTaxConfiguration(Province province);

    /// <summary>
    /// Gets all supported provinces
    /// </summary>
    Province[] GetSupportedProvinces();
}

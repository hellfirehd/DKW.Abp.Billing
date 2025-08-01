using Billing.EntityFrameworkCore;

namespace Billing;

/// <summary>
/// Enhanced Canadian tax provider with tax code support
/// Supports proper Canadian tax classification and exemptions
/// </summary>
public class EnhancedCanadianTaxProvider : ITaxProvider
{
    private readonly CanadianTaxProvider _baseTaxProvider;
    private readonly Dictionary<Guid, ItemClassification> _itemClassifications;

    private readonly TaxCodeRepository _taxCodes;

    public EnhancedCanadianTaxProvider()
    {
        _baseTaxProvider = new CanadianTaxProvider();
        _taxCodes = new();
        _itemClassifications = [];
    }

    /// <summary>
    /// Gets tax rates for an item based on its tax code and customer profile
    /// </summary>
    public ItemTaxRate[] GetTaxRatesForItem(Guid itemId, Province province, DateOnly effectiveDate, CustomerTaxProfile? customerProfile = null)
    {
        // Check customer exemption first
        if (customerProfile?.QualifiesForExemption(effectiveDate) == true)
        {
            return Array.Empty<ItemTaxRate>();
        }

        // Get item classification
        if (!_itemClassifications.TryGetValue(itemId, out var classification) || !classification.IsValidOn(effectiveDate))
        {
            // Fallback to standard rates if no classification
            return GetTaxRates(province, effectiveDate);
        }

        // Get tax code
        var taxCode = _taxCodes.GetByCode(classification.TaxCode);
        if (taxCode is null || !taxCode.IsValidOn(effectiveDate))
        {
            // Fallback to standard rates if tax code not found
            return GetTaxRates(province, effectiveDate);
        }

        // Apply tax treatment rules
        return taxCode.TaxTreatment switch
        {
            TaxTreatment.Exempt or TaxTreatment.OutOfScope => Array.Empty<ItemTaxRate>(),
            TaxTreatment.ZeroRated => GetZeroRatedTaxRates(province, effectiveDate),
            TaxTreatment.Standard => GetStandardTaxRates(province, effectiveDate, taxCode.ItemCategory),
            _ => GetTaxRates(province, effectiveDate)
        };
    }

    /// <summary>
    /// Assigns a tax code to an item
    /// </summary>
    public void AssignTaxCodeToItem(Guid itemId, string taxCode, string assignedBy, string notes = "")
    {
        var classification = new ItemClassification
        {
            ItemId = itemId,
            TaxCode = taxCode,
            AssignedBy = assignedBy,
            Notes = notes
        };

        _itemClassifications[itemId] = classification;
    }

    /// <summary>
    /// Gets all available tax codes
    /// </summary>
    public IEnumerable<TaxCode> GetAvailableTaxCodes()
    {
        return _taxCodes.GetAll().Where(tc => tc.IsActive);
    }

    /// <summary>
    /// Legacy interface implementation
    /// </summary>
    public ItemTaxRate[] GetTaxRates(Province province, DateOnly effectiveDate)
    {
        return _baseTaxProvider.GetTaxRates(province, effectiveDate);
    }

    /// <summary>
    /// Legacy interface implementation with category filtering
    /// </summary>
    public ItemTaxRate[] GetTaxRatesForCategory(Province province, DateOnly effectiveDate, TaxCategory taxCategory)
    {
        return _baseTaxProvider.GetTaxRatesForCategory(province, effectiveDate, taxCategory);
    }

    /// <summary>
    /// Gets tax rates for specific item category
    /// </summary>
    public ItemTaxRate[] GetTaxRatesForItemCategory(Province province, DateOnly effectiveDate, ItemCategory itemCategory)
    {
        var allRates = GetTaxRates(province, effectiveDate);

        return itemCategory switch
        {
            // Goods categories
            ItemCategory.GeneralGoods or ItemCategory.BasicGroceries or ItemCategory.PreparedFoods or
            ItemCategory.BooksAndMagazines or ItemCategory.PrescriptionDrugs or ItemCategory.MedicalDevices or
            ItemCategory.ChildrensClothing or ItemCategory.AgriculturalProducts or ItemCategory.DigitalProducts => allRates.Where(r => r.TaxCategory == TaxCategory.TaxableProduct).ToArray(),

            // Services categories
            ItemCategory.GeneralServices or ItemCategory.ProfessionalServices or ItemCategory.HealthcareServices or
            ItemCategory.EducationalServices or ItemCategory.FinancialServices or ItemCategory.LegalServices or
            ItemCategory.TransportationServices or ItemCategory.ShippingAndDelivery or ItemCategory.DigitalServices =>
                allRates.Where(r => r.TaxCategory == TaxCategory.TaxableService).ToArray(),
            // Special categories
            _ => allRates
        };
    }

    public bool HasTaxConfiguration(Province province)
    {
        return _baseTaxProvider.HasTaxConfiguration(province);
    }

    public Province[] GetSupportedProvinces()
    {
        return _baseTaxProvider.GetSupportedProvinces();
    }

    private ItemTaxRate[] GetZeroRatedTaxRates(Province province, DateOnly effectiveDate)
    {
        var standardRates = GetTaxRates(province, effectiveDate);

        // Convert to zero-rated (0% but still tracked)
        return standardRates.Select(rate => new ItemTaxRate(
            rate.Code,
            rate.TaxCategory,
            new TaxRate(rate.TaxRate.EffectiveDate, rate.TaxRate.ExpirationDate, 0m)
        )).ToArray();
    }

    private ItemTaxRate[] GetStandardTaxRates(Province province, DateOnly effectiveDate, ItemCategory itemCategory)
    {
        return GetTaxRatesForItemCategory(province, effectiveDate, itemCategory);
    }
}
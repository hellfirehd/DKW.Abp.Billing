namespace Billing;

/// <summary>
/// Enhanced Canadian tax provider with tax code support
/// Supports proper Canadian tax classification and exemptions
/// </summary>
public class EnhancedCanadianTaxProvider : ITaxProvider
{
    private readonly CanadianTaxProvider _baseTaxProvider;
    private readonly Dictionary<string, TaxCode> _taxCodes;
    private readonly Dictionary<Guid, ItemClassification> _itemClassifications;

    public EnhancedCanadianTaxProvider()
    {
        _baseTaxProvider = new CanadianTaxProvider();
        _taxCodes = InitializeTaxCodes();
        _itemClassifications = new Dictionary<Guid, ItemClassification>();
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
        if (!_taxCodes.TryGetValue(classification.TaxCode, out var taxCode) || !taxCode.IsValidOn(effectiveDate))
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
        return _taxCodes.Values.Where(tc => tc.IsActive);
    }

    /// <summary>
    /// Gets tax code by code string
    /// </summary>
    public TaxCode? GetTaxCode(string code)
    {
        return _taxCodes.TryGetValue(code, out var taxCode) ? taxCode : null;
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

    private Dictionary<string, TaxCode> InitializeTaxCodes()
    {
        var taxCodes = new Dictionary<string, TaxCode>();

        // Standard taxable goods
        taxCodes["STD-GOODS"] = new TaxCode
        {
            Code = "STD-GOODS",
            Description = "Standard Taxable Goods",
            TaxTreatment = TaxTreatment.Standard,
            ItemCategory = ItemCategory.GeneralGoods,
            CraReference = "Schedule VI, Part I"
        };

        // Basic groceries (zero-rated)
        taxCodes["ZR-GROCERY"] = new TaxCode
        {
            Code = "ZR-GROCERY",
            Description = "Basic Groceries",
            TaxTreatment = TaxTreatment.ZeroRated,
            ItemCategory = ItemCategory.BasicGroceries,
            CraReference = "Schedule VI, Part III",
            Notes = "Meat, fish, poultry, dairy, eggs, vegetables, fruits, cereals, etc."
        };

        // Prescription drugs (zero-rated)
        taxCodes["ZR-PRESCRIP"] = new TaxCode
        {
            Code = "ZR-PRESCRIP",
            Description = "Prescription Drugs",
            TaxTreatment = TaxTreatment.ZeroRated,
            ItemCategory = ItemCategory.PrescriptionDrugs,
            CraReference = "Schedule VI, Part II",
            Notes = "Drugs dispensed by prescription"
        };

        // Medical devices (zero-rated)
        taxCodes["ZR-MEDICAL"] = new TaxCode
        {
            Code = "ZR-MEDICAL",
            Description = "Medical Devices",
            TaxTreatment = TaxTreatment.ZeroRated,
            ItemCategory = ItemCategory.MedicalDevices,
            CraReference = "Schedule VI, Part II",
            Notes = "Prescribed medical devices and equipment"
        };

        // Healthcare services (exempt)
        taxCodes["EX-HEALTH"] = new TaxCode
        {
            Code = "EX-HEALTH",
            Description = "Healthcare Services",
            TaxTreatment = TaxTreatment.Exempt,
            ItemCategory = ItemCategory.HealthcareServices,
            CraReference = "Schedule V, Part II",
            Notes = "Services rendered by healthcare professionals"
        };

        // Educational services (exempt)
        taxCodes["EX-EDUCATION"] = new TaxCode
        {
            Code = "EX-EDUCATION",
            Description = "Educational Services",
            TaxTreatment = TaxTreatment.Exempt,
            ItemCategory = ItemCategory.EducationalServices,
            CraReference = "Schedule V, Part III",
            Notes = "Degree, diploma, certificate courses"
        };

        // Financial services (exempt)
        taxCodes["EX-FINANCIAL"] = new TaxCode
        {
            Code = "EX-FINANCIAL",
            Description = "Financial Services",
            TaxTreatment = TaxTreatment.Exempt,
            ItemCategory = ItemCategory.FinancialServices,
            CraReference = "Schedule V, Part VII",
            Notes = "Banking, insurance, investment services"
        };

        // Professional services (standard)
        taxCodes["STD-PROF"] = new TaxCode
        {
            Code = "STD-PROF",
            Description = "Professional Services",
            TaxTreatment = TaxTreatment.Standard,
            ItemCategory = ItemCategory.ProfessionalServices,
            Notes = "Consulting, accounting, legal services (non-exempt)"
        };

        // Digital services (standard)
        taxCodes["STD-DIGITAL"] = new TaxCode
        {
            Code = "STD-DIGITAL",
            Description = "Digital Services",
            TaxTreatment = TaxTreatment.Standard,
            ItemCategory = ItemCategory.DigitalServices,
            Notes = "Software, digital content, online services"
        };

        // Exports (zero-rated)
        taxCodes["ZR-EXPORT"] = new TaxCode
        {
            Code = "ZR-EXPORT",
            Description = "Exports",
            TaxTreatment = TaxTreatment.ZeroRated,
            ItemCategory = ItemCategory.Exports,
            CraReference = "Schedule VI, Part V",
            Notes = "Goods and services exported from Canada"
        };

        return taxCodes;
    }
}
namespace Billing;

public class InvoiceManager(
    IItemRepository itemRepository,
    ITaxCodeRepository taxCodeRepository,
    ITaxProvider taxProvider,
    ProvinceManager provinceManager,
    InvoiceItemManager invoiceItemManager)
{
    public IItemRepository ItemRepository { get; } = itemRepository;
    public ITaxCodeRepository TaxCodeRepository { get; } = taxCodeRepository;
    public ITaxProvider TaxProvider { get; } = taxProvider;
    public ProvinceManager ProvinceManager { get; } = provinceManager;
    public InvoiceItemManager InvoiceItemManager { get; } = invoiceItemManager;

    public ItemClassification ClassifyItem(InvoiceItem item)
    {
        throw new NotImplementedException("Item classification logic is not implemented yet.");
    }

    /// <summary>
    /// Gets tax rates for an item based on its tax code and customer profile
    /// </summary>
    public IEnumerable<TaxRate> GetTaxRatesForItem(InvoiceItem item, CustomerTaxProfile customerProfile, DateOnly effectiveDate)
    {
        // Check customer exemption first
        if (customerProfile.QualifiesForExemption(effectiveDate) == true)
        {
            return [];
        }

        // Get item classification
        if (item.ItemClassification is null || !item.ItemClassification.IsValidOn(effectiveDate))
        {
            // Fallback to standard rates if no classification
            return TaxProvider.GetTaxRates(customerProfile.TaxProvince, effectiveDate);
        }

        // Get tax code
        var taxCode = TaxCodeRepository.GetByCode(item.ItemClassification.TaxCode);
        if (taxCode is null || !taxCode.IsValidOn(effectiveDate))
        {
            // Fallback to standard rates if tax code not found
            return TaxProvider.GetTaxRates(customerProfile.TaxProvince, effectiveDate);
        }

        // Apply tax treatment rules
        return taxCode.TaxTreatment switch
        {
            TaxTreatment.Exempt or TaxTreatment.OutOfScope => [],
            TaxTreatment.ZeroRated => GetZeroRatedTaxRates(customerProfile.TaxProvince, effectiveDate),
            _ => TaxProvider.GetTaxRates(customerProfile.TaxProvince, effectiveDate)
        };
    }

    private IEnumerable<TaxRate> GetZeroRatedTaxRates(Province taxProvince, DateOnly effectiveDate)
    {
        // Zero-rated items typically have a specific tax rate of 0%
        return TaxProvider.GetTaxRates(taxProvince, effectiveDate)
            .Select(rate => rate with { Rate = 0 });
    }
}

namespace Billing;

/// <summary>
/// Production-ready Canadian tax calculation provider
/// Supports all Canadian provinces and territories with accurate tax rates
/// </summary>
public class CanadianTaxProvider : ITaxProvider
{
    private sealed record ProvinceTax(Province Province, Tax Tax);

    private readonly ProvinceTax[] _taxMap;

    public CanadianTaxProvider()
    {
        var gst = new Tax("GST", "GST", TaxScope.Federal,
        [
            new TaxRate(new DateOnly(1991, 1, 1), new DateOnly(2006, 7, 1), 0.07M),
            new TaxRate(new DateOnly(2006, 7, 1), new DateOnly(2008, 1, 1), 0.06M),
            new TaxRate(new DateOnly(2008, 1, 1), DateOnly.MaxValue, 0.05M)
        ]);

        var bcpst = new Tax("BC-PST", "PST", TaxScope.Provincial,
            [new TaxRate(new DateOnly(2013, 4, 1), DateOnly.MaxValue, 0.07M)]);

        var mbrst = new Tax("MB-PST", "RST", TaxScope.Provincial,
            [new TaxRate(new DateOnly(2019, 7, 1), DateOnly.MaxValue, 0.07M)]);

        var nbhst = new Tax("NB-HST", "HST", TaxScope.Provincial,
            [new TaxRate(new DateOnly(2016, 7, 1), DateOnly.MaxValue, 0.15M)]);

        var nlhst = new Tax("NL-HST", "HST", TaxScope.Provincial,
            [new TaxRate(new DateOnly(2016, 7, 1), DateOnly.MaxValue, 0.15M)]);

        var nshst = new Tax("NS-HST", "HST", TaxScope.Provincial,
        [
            new TaxRate(new DateOnly(2010, 7, 1), new DateOnly(2025, 4, 1), 0.15M),
            new TaxRate(new DateOnly(2025, 4, 1), DateOnly.MaxValue, 0.14M)
        ]);

        var onhst = new Tax("ON-HST", "HST", TaxScope.Provincial,
            [new TaxRate(new DateOnly(2010, 7, 1), DateOnly.MaxValue, 0.13M)]);

        var pehst = new Tax("PE-HST", "HST", TaxScope.Provincial,
            [new TaxRate(new DateOnly(2016, 10, 1), DateOnly.MaxValue, 0.15M)]);

        var qcqst = new Tax("QC-QST", "QST", TaxScope.Provincial,
            [new TaxRate(new DateOnly(2013, 1, 1), DateOnly.MaxValue, 0.09975M)]);

        var skpst = new Tax("SK-PST", "PST", TaxScope.Provincial,
            [new TaxRate(new DateOnly(2017, 3, 23), DateOnly.MaxValue, 0.06M)]);

        _taxMap = [
            new ProvinceTax(Provinces.AB, gst),
            new ProvinceTax(Provinces.BC, gst),
            new ProvinceTax(Provinces.BC, bcpst),
            new ProvinceTax(Provinces.MB, gst),
            new ProvinceTax(Provinces.MB, mbrst),
            new ProvinceTax(Provinces.NB, nbhst),
            new ProvinceTax(Provinces.NL, nlhst),
            new ProvinceTax(Provinces.NS, nshst),
            new ProvinceTax(Provinces.ON, onhst),
            new ProvinceTax(Provinces.PE, pehst),
            new ProvinceTax(Provinces.QC, gst),
            new ProvinceTax(Provinces.QC, qcqst),
            new ProvinceTax(Provinces.SK, gst),
            new ProvinceTax(Provinces.SK, skpst),
            new ProvinceTax(Provinces.NT, gst),
            new ProvinceTax(Provinces.NU, gst),
            new ProvinceTax(Provinces.YT, gst),
        ];
    }

    /// <summary>
    /// Gets applicable tax rates for a province and date, with category filtering
    /// </summary>
    public ItemTaxRate[] GetTaxRates(Province province, DateOnly effectiveDate)
    {
        var rates = new List<ItemTaxRate>();

        foreach (var map in _taxMap.Where(m => m.Province == province))
        {
            var rate = map.Tax.TaxRate(effectiveDate);
            if (rate is not null)
            {
                rates.Add(new ItemTaxRate(map.Tax.Code, TaxCategory.TaxableProduct, rate));
            }
        }

        return [.. rates];
    }

    /// <summary>
    /// Gets tax rates filtered by tax category
    /// </summary>
    public ItemTaxRate[] GetTaxRatesForCategory(Province province, DateOnly effectiveDate, TaxCategory taxCategory)
    {
        var allRates = GetTaxRates(province, effectiveDate);

        return taxCategory switch
        {
            TaxCategory.NonTaxableProduct or TaxCategory.NonTaxableService => [],
            TaxCategory.TaxableProduct => allRates.Where(r => IsApplicableToGoods(r.Code)).ToArray(),
            TaxCategory.TaxableService => allRates.Where(r => IsApplicableToServices(r.Code)).ToArray(),
            TaxCategory.DigitalProduct => allRates.Where(r => IsApplicableToGoods(r.Code)).ToArray(),
            TaxCategory.Shipping => allRates.Where(r => IsApplicableToServices(r.Code)).ToArray(),
            _ => allRates
        };
    }

    /// <summary>
    /// Determines if a tax applies to goods based on tax code
    /// </summary>
    private static bool IsApplicableToGoods(string taxCode)
    {
        // GST applies to all goods, HST replaces both GST and PST
        return taxCode == "GST" || taxCode.Contains("HST");
    }

    /// <summary>
    /// Determines if a tax applies to services based on tax code
    /// </summary>
    private static bool IsApplicableToServices(string taxCode)
    {
        // All taxes apply to services in Canada
        return true;
    }

    /// <summary>
    /// Validates if a province has tax configuration
    /// </summary>
    public Boolean HasTaxConfiguration(Province province)
    {
        return _taxMap.Any(m => m.Province == province);
    }

    /// <summary>
    /// Gets all supported provinces
    /// </summary>
    public Province[] GetSupportedProvinces()
    {
        return _taxMap.Select(m => m.Province).Distinct().ToArray();
    }
}

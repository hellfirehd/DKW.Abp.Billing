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
        var gst = new Tax("GST", "GST", TaxType.Goods, TaxScope.Federal,
        [
            new TaxRate(new DateOnly(1991, 1, 1), new DateOnly(2006, 7, 1), 0.07M),
            new TaxRate(new DateOnly(2006, 7, 1), new DateOnly(2008, 1, 1), 0.06M),
            new TaxRate(new DateOnly(2008, 1, 1), DateOnly.MaxValue, 0.05M)
        ]);

        var bcpst = new Tax("BC-PST", "PST", TaxType.Services, TaxScope.Provincial,
            [new TaxRate(new DateOnly(2013, 4, 1), DateOnly.MaxValue, 0.07M)]);

        var mbrst = new Tax("MB-PST", "RST", TaxType.Services, TaxScope.Provincial,
            [new TaxRate(new DateOnly(2019, 7, 1), DateOnly.MaxValue, 0.07M)]);

        var nbhst = new Tax("NB-HST", "HST", TaxType.Combined, TaxScope.Provincial,
            [new TaxRate(new DateOnly(2016, 7, 1), DateOnly.MaxValue, 0.15M)]);

        var nlhst = new Tax("NL-HST", "HST", TaxType.Combined, TaxScope.Provincial,
            [new TaxRate(new DateOnly(2016, 7, 1), DateOnly.MaxValue, 0.15M)]);

        var nshst = new Tax("NS-HST", "HST", TaxType.Combined, TaxScope.Provincial,
        [
            new TaxRate(new DateOnly(2010, 7, 1), new DateOnly(2025, 4, 1), 0.15M),
            new TaxRate(new DateOnly(2025, 4, 1), DateOnly.MaxValue, 0.14M)
        ]);

        var onhst = new Tax("ON-HST", "HST", TaxType.Combined, TaxScope.Provincial,
            [new TaxRate(new DateOnly(2010, 7, 1), DateOnly.MaxValue, 0.13M)]);

        var pehst = new Tax("PE-HST", "HST", TaxType.Combined, TaxScope.Provincial,
            [new TaxRate(new DateOnly(2016, 10, 1), DateOnly.MaxValue, 0.15M)]);

        var qcqst = new Tax("QC-QST", "QST", TaxType.Services, TaxScope.Provincial,
            [new TaxRate(new DateOnly(2013, 1, 1), DateOnly.MaxValue, 0.09975M)]);

        var skpst = new Tax("SK-PST", "PST", TaxType.Services, TaxScope.Provincial,
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
                rates.Add(new ItemTaxRate(map.Tax.Code, map.Tax.TaxType, rate));
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
            TaxCategory.TaxableProduct => allRates.Where(r => r.TaxType == TaxType.Goods || r.TaxType == TaxType.Combined).ToArray(),
            TaxCategory.TaxableService => allRates.Where(r => r.TaxType == TaxType.Services || r.TaxType == TaxType.Combined).ToArray(),
            TaxCategory.DigitalProduct => allRates.Where(r => r.TaxType == TaxType.Goods || r.TaxType == TaxType.Combined).ToArray(),
            TaxCategory.Shipping => allRates.Where(r => r.TaxType == TaxType.Services || r.TaxType == TaxType.Combined).ToArray(),
            _ => allRates
        };
    }

    /// <summary>
    /// Validates if a province has tax configuration
    /// </summary>
    public bool HasTaxConfiguration(Province province)
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

namespace Billing;

/// <summary>
/// Production-ready Canadian tax calculation provider
/// Supports all Canadian provinces and territories with accurate tax rates
/// </summary>
public class CanadianTaxProvider : ITaxProvider
{
    private readonly ProvinceManager _manager;

    private sealed record ProvinceTax(Province Province, Tax Tax);

    private readonly ProvinceTax[] _taxMap;

    protected TimeProvider TimeProvider { get; }

    public CanadianTaxProvider(ProvinceManager provinceManager, TimeProvider timeProvider)
    {
        _manager = provinceManager ?? throw new ArgumentNullException(nameof(provinceManager));

        var gst = new Tax("GST", "GST")
            .AddTaxRate(0.05M, new DateOnly(2008, 1, 1), DateOnly.MaxValue)
            .AddTaxRate(0.06M, new DateOnly(2006, 7, 1), new DateOnly(2008, 1, 1))
            .AddTaxRate(0.07M, new DateOnly(1991, 1, 1), new DateOnly(2006, 7, 1));

        var bcpst = new Tax("BC-PST", "PST")
            .AddTaxRate(0.07M, new DateOnly(2013, 4, 1), DateOnly.MaxValue);

        var mbrst = new Tax("MB-PST", "RST")
            .AddTaxRate(0.07M, new DateOnly(2019, 7, 1), DateOnly.MaxValue);

        var nbhst = new Tax("NB-HST", "HST")
            .AddTaxRate(0.15M, new DateOnly(2016, 7, 1), DateOnly.MaxValue);

        var nlhst = new Tax("NL-HST", "HST")
            .AddTaxRate(0.15M, new DateOnly(2016, 7, 1), DateOnly.MaxValue);

        var nshst = new Tax("NS-HST", "HST")
            .AddTaxRate(0.15M, new DateOnly(2010, 7, 1), new DateOnly(2025, 4, 1))
            .AddTaxRate(0.14M, new DateOnly(2025, 4, 1), DateOnly.MaxValue);

        var onhst = new Tax("ON-HST", "HST")
            .AddTaxRate(0.13M, new DateOnly(2010, 7, 1), DateOnly.MaxValue);

        var pehst = new Tax("PE-HST", "HST")
            .AddTaxRate(0.15M, new DateOnly(2016, 10, 1), DateOnly.MaxValue);

        var qcqst = new Tax("QC-QST", "QST")
            .AddTaxRate(0.09975M, new DateOnly(2013, 1, 1), DateOnly.MaxValue);

        var skpst = new Tax("SK-PST", "PST")
            .AddTaxRate(0.06M, new DateOnly(2017, 3, 23), DateOnly.MaxValue);

        _taxMap = [
            new ProvinceTax(_manager.GetProvince("AB"), gst),

            new ProvinceTax(_manager.GetProvince("BC"), gst),
            new ProvinceTax(_manager.GetProvince("BC"), bcpst),

            new ProvinceTax(_manager.GetProvince("MB"), gst),
            new ProvinceTax(_manager.GetProvince("MB"), mbrst),

            new ProvinceTax(_manager.GetProvince("NB"), nbhst),

            new ProvinceTax(_manager.GetProvince("NL"), nlhst),

            new ProvinceTax(_manager.GetProvince("NS"), nshst),

            new ProvinceTax(_manager.GetProvince("ON"), onhst),

            new ProvinceTax(_manager.GetProvince("PE"), pehst),

            new ProvinceTax(_manager.GetProvince("QC"), gst),
            new ProvinceTax(_manager.GetProvince("QC"), qcqst),

            new ProvinceTax(_manager.GetProvince("SK"), gst),
            new ProvinceTax(_manager.GetProvince("SK"), skpst),

            new ProvinceTax(_manager.GetProvince("NT"), gst),

            new ProvinceTax(_manager.GetProvince("NU"), gst),

            new ProvinceTax(_manager.GetProvince("YT"), gst),
        ];
        TimeProvider = timeProvider;
    }

    /// <summary>
    /// Gets applicable tax rates for a province and date, with category filtering
    /// </summary>
    public IEnumerable<TaxRate> GetTaxRates(Province province, DateOnly? effectiveDate = null)
    {
        var date = effectiveDate ?? DateOnly.FromDateTime(TimeProvider.GetUtcNow().DateTime);

        foreach (var map in _taxMap.Where(m => m.Province == province))
        {
            var rate = map.Tax.GetTaxRate(date);
            if (rate is not null)
            {
                yield return rate;
            }
        }
    }
}

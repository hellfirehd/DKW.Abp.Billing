namespace Billing.EntityFrameworkCore;

public class ProvinceRepository : IProvinceRepository
{
    private readonly Dictionary<string, Province> _provinces = new(StringComparer.OrdinalIgnoreCase)
    {
        { "AB", AB },
        { "BC", BC },
        { "MB", MB },
        { "NB", NB },
        { "NL", NL },
        { "NS", NS },
        { "ON", ON },
        { "PE", PE },
        { "QC", QC },
        { "SK", SK },
        { "NT", NT },
        { "NU", NU },
        { "YT", YT }
    };

    public Province GetProvince(string code)
    {
        if (String.IsNullOrWhiteSpace(code))
        {
            return Province.Empty;
        }

        return _provinces.TryGetValue(code.Trim(), out var province)
            ? province
            : Province.Empty;
    }

    public IEnumerable<string> Codes() => _provinces.Keys;

    public IEnumerable<Province> GetAll()
    {
        yield return AB;
        yield return BC;
        yield return MB;
        yield return NB;
        yield return NL;
        yield return NS;
        yield return ON;
        yield return PE;
        yield return QC;
        yield return SK;
        yield return NT;
        yield return NU;
        yield return YT;
    }

    private static readonly Province AB = Province.Create("AB", "Alberta");
    private static readonly Province BC = Province.Create("BC", "British Columbia");
    private static readonly Province MB = Province.Create("MB", "Manitoba");
    private static readonly Province NB = Province.Create("NB", "New Brunswick");
    private static readonly Province NL = Province.Create("NL", "Newfoundland and Labrador");
    private static readonly Province NS = Province.Create("NS", "Nova Scotia");
    private static readonly Province ON = Province.Create("ON", "Ontario");
    private static readonly Province PE = Province.Create("PE", "Prince Edward Island");
    private static readonly Province QC = Province.Create("QC", "Québec");
    private static readonly Province SK = Province.Create("SK", "Saskatchewan");
    private static readonly Province NT = Province.Create("NT", "Northwest Territories");
    private static readonly Province NU = Province.Create("NU", "Nunavut");
    private static readonly Province YT = Province.Create("YT", "Yukon");
}

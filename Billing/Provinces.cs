using System.Globalization;
using System.Text.RegularExpressions;

namespace Billing;

public static partial class Provinces
{
    public static readonly String[] Codes = ["AB", "BC", "MB", "NB", "NL", "NS", "ON", "PE", "QC", "SK", "NT", "NU", "YT"];

    public static IEnumerable<Province> All
    {
        get
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
    }

    public static readonly Province AB = Province.Create("AB", "Alberta");
    public static readonly Province BC = Province.Create("BC", "British Columbia");
    public static readonly Province MB = Province.Create("MB", "Manitoba");
    public static readonly Province NB = Province.Create("NB", "New Brunswick");
    public static readonly Province NL = Province.Create("NL", "Newfoundland and Labrador");
    public static readonly Province NS = Province.Create("NS", "Nova Scotia");
    public static readonly Province ON = Province.Create("ON", "Ontario");
    public static readonly Province PE = Province.Create("PE", "Prince Edward Island");
    public static readonly Province QC = Province.Create("QC", "Québec");
    public static readonly Province SK = Province.Create("SK", "Saskatchewan");
    public static readonly Province NT = Province.Create("NT", "Northwest Territories");
    public static readonly Province NU = Province.Create("NU", "Nunavut");
    public static readonly Province YT = Province.Create("YT", "Yukon");

    public static String ToProvince(this String? province)
    {
        if (String.IsNullOrWhiteSpace(province))
        {
            return String.Empty;
        }

        var prov = Parse(province, false);
        foreach (var p in All)
        {
            if (p == prov)
            {
                return p.Code;
            }
        }

        return province;
    }

    public static Boolean TryParse(String? code, out Province province)
    {
        province = Parse(code, false);
        return province != Province.Empty;
    }

    public static Province Parse(String? code, Boolean throwOnInvalid = true)
    {
        if (String.IsNullOrWhiteSpace(code))
        {
            return throwOnInvalid
                ? throw new ArgumentException($"'{nameof(code)}' cannot be null or whitespace.", nameof(code))
                : Province.Empty;
        }

        var normalized = Replacer().Replace(code, String.Empty).ToUpper(CultureInfo.InvariantCulture);

        return normalized switch
        {
            "AB" => Provinces.AB,
            "ALBERA" => Provinces.AB,
            "ALBERTA" => Provinces.AB,
            "BC" => Provinces.BC,
            "BRITISH COLUMBIA" => Provinces.BC,
            "MB" => Provinces.MB,
            "MANITOBA" => Provinces.MB,
            "NB" => Provinces.NB,
            "NEW BRUNSWICK" => Provinces.NB,
            "NL" => Provinces.NL,
            "NEWFOUNDLAND" => Provinces.NL,
            "NEWFOUNDLAND AND LABRADOR" => Provinces.NL,
            "NS" => Provinces.NS,
            "NOVA SCOTIA" => Provinces.NS,
            "ON" => Provinces.ON,
            "ONTARIO" => Provinces.ON,
            "PE" => Provinces.PE,
            "PRINCE EDWARD ISLAND" => Provinces.PE,
            "QC" => Provinces.QC,
            "QUBEC" => Provinces.QC,
            "QUÉBEC" => Provinces.QC,
            "QUEBEC" => Provinces.QC,
            "SK" => Provinces.SK,
            "SASKATCHEWAN" => Provinces.SK,
            "NT" => Provinces.NT,
            "NORTHWEST TERRITORIES" => Provinces.NT,
            "NU" => Provinces.NU,
            "NUNAVUT" => Provinces.NU,
            "YT" => Provinces.YT,
            "YUKON" => Provinces.YT,
            _ => throwOnInvalid
                ? throw new ArgumentOutOfRangeException(nameof(code), code, "Invalid province code")
                : Province.Empty
        };
    }

    [GeneratedRegex("[^A-Z ]", RegexOptions.IgnoreCase)]
    private static partial Regex Replacer();
}

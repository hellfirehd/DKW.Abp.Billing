using System.Globalization;
using System.Text.RegularExpressions;

namespace Billing;

public partial class ProvinceManager(IProvinceRepository repository)
{
    private readonly IProvinceRepository _repository = repository;

    public Province GetProvince(String? code)
    {
        if (String.IsNullOrWhiteSpace(code))
        {
            return Province.Empty;
        }

        return Parse(code, true);
    }

    public IEnumerable<Province> GetAllProvinces()
    {
        return _repository.GetAll();
    }

    public Boolean IsValidProvince(String? code)
    {
        return TryParse(code, out _);
    }

    public Boolean TryParse(String? code, out Province province)
    {
        province = Parse(code, false);
        return province != Province.Empty;
    }

    public Province Parse(String? code, Boolean throwOnInvalid = true)
    {
        if (String.IsNullOrWhiteSpace(code))
        {
            return throwOnInvalid
                ? throw new ArgumentException($"'{nameof(code)}' cannot be null or whitespace.", nameof(code))
                : Province.Empty;
        }

        var normalized = Replacer().Replace(code, String.Empty).ToUpper(CultureInfo.InvariantCulture);

        var match = normalized switch
        {
            "AB" => "AB",
            "ALBERA" => "AB",
            "ALBERTA" => "AB",
            "BC" => "BC",
            "BRITISH COLUMBIA" => "BC",
            "MB" => "MB",
            "MANITOBA" => "MB",
            "NB" => "NB",
            "NEW BRUNSWICK" => "NB",
            "NL" => "NL",
            "NEWFOUNDLAND" => "NL",
            "NEWFOUNDLAND AND LABRADOR" => "NL",
            "NS" => "NS",
            "NOVA SCOTIA" => "NS",
            "ON" => "ON",
            "ONTARIO" => "ON",
            "PE" => "PE",
            "PRINCE EDWARD ISLAND" => "PE",
            "QC" => "QC",
            "QUBEC" => "QC",
            "QUÉBEC" => "QC",
            "QUEBEC" => "QC",
            "SK" => "SK",
            "SASKATCHEWAN" => "SK",
            "NT" => "NT",
            "NORTHWEST TERRITORIES" => "NT",
            "NU" => "NU",
            "NUNAVUT" => "NU",
            "YT" => "YT",
            "YUKON" => "YT",
            _ => throwOnInvalid
                ? throw new ArgumentOutOfRangeException(nameof(code), code, "Invalid province code")
                : Province.Empty
        };

        return _repository.GetProvince(match)
            ?? throw new ArgumentOutOfRangeException(nameof(code), code, "Invalid province code");
    }

    [GeneratedRegex("[^A-Z ]", RegexOptions.IgnoreCase)]
    private static partial Regex Replacer();
}
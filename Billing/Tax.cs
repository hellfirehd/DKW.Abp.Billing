using System.Security.Cryptography;
using System.Text;

namespace Billing;

/// <summary>
/// Represents a tax that changes over time, such as GST or VAT.
/// </summary>
public class Tax(Guid id, string code, string name)
{
    private readonly List<TaxRate> _rates = [];

    public Guid Id { get; init; } = id;
    public String Code { get; } = code;
    public String Name { get; set; } = name;
    public IReadOnlyCollection<TaxRate> Rates => _rates.AsReadOnly();

    // Deterministically generate a Guid from code and name
    private static Guid CreateDeterministicId(string code)
    {
        // Use a namespace Guid (arbitrary, but fixed for this purpose)
        var namespaceGuid = Guid.Parse("b6a7b810-9dad-11d1-80b4-00c04fd430c8");
        var nameBytes = Encoding.UTF8.GetBytes(code);
        // Use SHA1 to hash namespace + name (UUID v5 style)
        var nsBytes = namespaceGuid.ToByteArray();
        var data = nsBytes.Concat(nameBytes).ToArray();
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(data);
        var newGuid = new byte[16];
        Array.Copy(hash, newGuid, 16);
        // Set version to 5 (name-based SHA1)
        newGuid[6] = (byte)((newGuid[6] & 0x0F) | 0x50);
        // Set variant
        newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);
        return new Guid(newGuid);
    }

    public Tax(string code, string name)
        : this(CreateDeterministicId(code), code, name) { }

    public Tax AddTaxRate(Decimal rate, DateOnly effectiveDate, DateOnly? expirationDate = null)
    {
        var newRate = new TaxRate(this, rate, effectiveDate, expirationDate);
        _rates.Add(newRate);
        return this;
    }

    /// <summary>
    /// Retrieves the applicable tax rate for a given date.
    /// </summary>
    /// <remarks>
    /// The method searches for the most recent tax rate that is effective on or before the specified
    /// date and has not expired. If no matching tax rate is found, the method returns <see langword="null"/>.
    /// </remarks>
    /// <param name="date">The date for which to determine the tax rate.</param>
    /// <returns>The tax rate that is effective on the specified date, or <see langword="null"/> if no tax rate is applicable.</returns>
    public TaxRate? GetTaxRate(DateOnly date)
        => Rates.OrderByDescending(r => r.EffectiveDate)
        .FirstOrDefault(tr => tr.EffectiveDate <= date && (!tr.ExpirationDate.HasValue || date <= tr.ExpirationDate));
}

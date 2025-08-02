// DKW ABP Framework Extensions
// Copyright (C) 2025 Doug Wilson
//
// This program is free software: you can redistribute it and/or modify it under the terms of
// the GNU Affero General Public License as published by the Free Software Foundation, either
// version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License along with this
// program. If not, see <https://www.gnu.org/licenses/>.

using System.Security.Cryptography;
using System.Text;

namespace Dkw.BillingManagement;

/// <summary>
/// Represents a tax that changes over time, such as GST or VAT.
/// </summary>
public class Tax(Guid id, String code, String name, TaxJurisdiction jurisdiction)
{
    private readonly List<TaxRate> _rates = [];

    public Guid Id { get; init; } = id;
    public String Code { get; } = code;
    public String Name { get; set; } = name;
    public TaxJurisdiction Jurisdiction { get; set; } = jurisdiction;

    public IReadOnlyCollection<TaxRate> Rates => _rates.AsReadOnly();

    // Deterministically generate a Guid from code and name
    private static Guid CreateDeterministicId(String code)
    {
        // Use a namespace Guid (arbitrary, but fixed for this purpose)
        var namespaceGuid = Guid.Parse("b6a7b810-9dad-11d1-80b4-00c04fd430c8");
        var nameBytes = Encoding.UTF8.GetBytes(code);
        // Use SHA1 to hash namespace + name (UUID v5 style)
        var nsBytes = namespaceGuid.ToByteArray();
        var data = nsBytes.Concat(nameBytes).ToArray();
        var hash = SHA256.HashData(data);
        var newGuid = new Byte[16];
        Array.Copy(hash, newGuid, 16);
        // Set version to 5 (name-based SHA1)
        newGuid[6] = (Byte)(newGuid[6] & 0x0F | 0x50);
        // Set variant
        newGuid[8] = (Byte)(newGuid[8] & 0x3F | 0x80);
        return new Guid(newGuid);
    }

    public Tax(String code, String name, TaxJurisdiction jurisdiction)
        : this(CreateDeterministicId(code), code, name, jurisdiction) { }

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

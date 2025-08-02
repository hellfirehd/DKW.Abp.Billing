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

using System.Diagnostics;

namespace Dkw.Abp.Billing;

/// <summary>
/// Represents a tax rate for a specific date range, 
/// </summary>
[DebuggerDisplay("{ToString()}")]
public record TaxRate
{
    private TaxRate()
    {
        // Parameterless constructor for EF Core/JSON serialization
    }

    public TaxRate(Tax tax, Decimal rate, DateOnly effectiveDate, DateOnly? expirationDate = null)
    {
        Tax = tax ?? throw new ArgumentNullException(nameof(tax));
        Rate = rate < 0
        ? throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be non-negative.")
        : rate;

        EffectiveDate = effectiveDate < DateOnly.FromDateTime(DateTime.UnixEpoch)
            ? throw new ArgumentOutOfRangeException(nameof(effectiveDate), "Effective Date is too far in the past.")
            : effectiveDate;

        ExpirationDate = expirationDate;
    }

    public Tax Tax { get; } = default!;
    public Decimal Rate { get; init; } // Allow init to facilitate Zero-Rated tax rates.

    public Boolean IsActive { get; init; } = true;
    public DateOnly EffectiveDate { get; }
    public DateOnly? ExpirationDate { get; }

    public String Name => Tax.Name;
    public String Code => Tax.Code;

    public Boolean InEffect(DateOnly date)
        => EffectiveDate <= date
        && (!ExpirationDate.HasValue || date <= ExpirationDate.Value);

    public override String ToString()
        => $"Tax: {Name}, Rate: {Rate}, Active: {IsActive}, Effective: {EffectiveDate}, Expiration: {ExpirationDate?.ToString(CultureInfo.InvariantCulture) ?? "N/A"}";

    public static TaxRate Create(Tax tax, Decimal rate, DateOnly effectiveDate, DateOnly? expirationDate = null)
        => new(tax, rate, effectiveDate, expirationDate);
}

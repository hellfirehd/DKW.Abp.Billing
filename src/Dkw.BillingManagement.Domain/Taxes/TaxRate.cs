// DKW Billing Management
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
using Volo.Abp.Domain.Entities;

namespace Dkw.BillingManagement.Taxes;

/// <summary>
/// Represents a tax rate for a specific date range, 
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class TaxRate : Entity<Guid>
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

        ExpiryDate = expirationDate;
    }

    public Decimal Rate { get; init; } // Allow init to facilitate Zero-Rated tax rates.
    public Boolean IsActive { get; init; } = true;
    public DateOnly EffectiveDate { get; }
    public DateOnly? ExpiryDate { get; }

    // Navigation properties
    public Guid TaxId { get; init; }
    public Tax Tax { get; init; } = default!;

    public String Name => Tax.Name;
    public String Code => Tax.Code;

    public Boolean IsInEffectOn(DateOnly date)
        => EffectiveDate <= date
        && (!ExpiryDate.HasValue || date <= ExpiryDate.Value);

    public override String ToString()
        => $"Tax: {Name}, Rate: {Rate}, Active: {IsActive}, Effective: {EffectiveDate}, Expiration: {ExpiryDate?.ToString(CultureInfo.InvariantCulture) ?? "N/A"}";

    public static TaxRate Create(Tax tax, Decimal rate, DateOnly effectiveDate, DateOnly? expirationDate = null)
        => new(tax, rate, effectiveDate, expirationDate);
}

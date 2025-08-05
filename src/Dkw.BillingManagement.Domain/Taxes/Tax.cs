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

using Dkw.BillingManagement.Invoices;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dkw.BillingManagement.Taxes;

/// <summary>
/// Represents a tax that changes over time, such as GST or VAT.
/// </summary>
public class Tax(Guid id, String name, String code) : FullAuditedEntity<Guid>(id)
{
    protected Tax() : this(default, default!, default!)
    {
    }

    private readonly List<TaxRate> _rates = [];

    public virtual String Name { get; set; } = name;

    public virtual String Code { get; } = code;

    public virtual IReadOnlyCollection<TaxRate> Rates => _rates;

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
    public ApplicableTax? GetTaxRate(DateOnly date)
        => Rates.OrderByDescending(r => r.EffectiveDate)
        .Where(tr => tr.EffectiveDate <= date && (!tr.ExpiryDate.HasValue || date <= tr.ExpiryDate))
        .Select(tr => new ApplicableTax(Id, Name, Code, tr.Rate))
        .FirstOrDefault();
}

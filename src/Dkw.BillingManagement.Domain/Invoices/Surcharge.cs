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

using Volo.Abp.Domain.Values;

namespace Dkw.BillingManagement.Invoices;

/// <summary>
/// Represents a surcharge applied to an order
/// </summary>
public class Surcharge : ValueObject
{
    public String Name { get; set; } = String.Empty;
    public String Description { get; set; } = String.Empty;
    public SurchargeType Type { get; set; }
    public Decimal FixedAmount { get; set; }
    public Decimal PercentageRate { get; set; } // As Decimal (e.g., 0.029 for 2.9%)
    public Boolean IsActive { get; set; } = true;

    /// <summary>
    /// Calculates the surcharge amount based on the order total
    /// </summary>
    public Decimal CalculateSurchargeAmount(Decimal orderTotal)
    {
        if (!IsActive)
        {
            return 0;
        }

        var surchargeAmount = FixedAmount + orderTotal * PercentageRate;
        return Math.Round(surchargeAmount, 2);
    }

    protected override IEnumerable<Object> GetAtomicValues()
    {
        yield return Name;
        yield return Description;
        yield return Type;
        yield return FixedAmount;
        yield return PercentageRate;
        yield return IsActive;
    }
}

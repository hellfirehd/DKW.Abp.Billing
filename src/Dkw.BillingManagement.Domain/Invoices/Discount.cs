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
using Volo.Abp.Domain.Entities.Auditing;

namespace Dkw.BillingManagement.Invoices;

/// <summary>
/// Represents a discount that can be applied to items or orders
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public class Discount : FullAuditedEntity<Guid>
{
    public String Name { get; set; } = String.Empty;
    public String Description { get; set; } = String.Empty;
    public DiscountType Type { get; set; }
    public DiscountScope Scope { get; set; }
    public Decimal Value { get; set; } // Percentage (0-1) or fixed amount
    public Decimal? MinimumAmount { get; set; } // Minimum order amount for discount to apply
    public Decimal? MaximumDiscount { get; set; } // Maximum discount amount
    public Boolean IsActive { get; set; } = true;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public override String ToString()
    {
        if (Type == DiscountType.Percentage)
        {
            return $"{Name} @ {Value:P}";
        }
        else
        {
            return $"{Name} = {Value:C}";
        }
    }

    /// <summary>
    /// Calculates the discount amount based on the base amount
    /// </summary>
    public Decimal GetDiscount(Decimal baseAmount)
    {
        if (!IsActive || StartDate.HasValue && DateOnly.FromDateTime(DateTime.UtcNow) < StartDate.Value ||
            EndDate.HasValue && DateOnly.FromDateTime(DateTime.UtcNow) > EndDate.Value)
        {
            return 0;
        }

        if (MinimumAmount.HasValue && baseAmount < MinimumAmount.Value)
        {
            return 0;
        }

        var discountAmount = Type == DiscountType.Percentage
            ? baseAmount * Value
            : Value;

        if (MaximumDiscount.HasValue && discountAmount > MaximumDiscount.Value)
        {
            discountAmount = MaximumDiscount.Value;
        }

        return Math.Round(discountAmount, 2);
    }
}

public class AppliedDiscount
{
    public String Name { get; set; } = String.Empty;

    public DiscountType Type { get; set; }
    public Decimal Value { get; set; } // Percentage (0-1) or fixed amount
    public Decimal EligibleAmount { get; set; }
    public Decimal Discount
        => Type == DiscountType.FixedAmount
            ? Math.Max(EligibleAmount, Math.Round(EligibleAmount - Value, 2))
            : Math.Round(EligibleAmount * Value, 2);

    public override String ToString() => $"{Name} @ {Value:P} on {EligibleAmount:c} = {Discount:c}";
}

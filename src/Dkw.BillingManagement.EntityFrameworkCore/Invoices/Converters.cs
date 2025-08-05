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

using Dkw.BillingManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dkw.BillingManagement.Invoices;

public class LineItemListValueConverter : JsonValueConverter<IReadOnlyList<LineItem>>;

public class LineItemListValueComparer : ValueComparer<IReadOnlyList<LineItem>>
{
    public LineItemListValueComparer()
        : base(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly()
        )
    { }
}

public class DiscountListConverter : JsonValueConverter<IReadOnlyList<Discount>>;

public class DiscountListValueComparer : ValueComparer<IReadOnlyList<Discount>>
{
    public DiscountListValueComparer()
        : base(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly()
        )
    { }
}

public class SurchargeListConverter : JsonValueConverter<IReadOnlyList<Surcharge>>;

public class SurchargeValueComparer : ValueComparer<IReadOnlyList<Surcharge>>
{
    public SurchargeValueComparer()
        : base(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly()
        )
    {
    }
}

public class AppliedTaxValueConverter : JsonValueConverter<AppliedTax>;

public class AppliedTaxListConverter : JsonValueConverter<IReadOnlyList<AppliedTax>>;

public class AppliedTaxListValueComparer : ValueComparer<IReadOnlyList<AppliedTax>>
{
    public AppliedTaxListValueComparer()
        : base(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly()
        )
    { }
}

public class ItemInfoConverter : JsonValueConverter<ItemInfo>;

public class ApplicableTaxListValueConverter : JsonValueConverter<IReadOnlyList<ApplicableTax>>;

public class ApplicableTaxListValueComparer : ValueComparer<IReadOnlyList<ApplicableTax>>
{
    public ApplicableTaxListValueComparer()
        : base(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly()
        )
    { }
}

public class RefundListConverter : JsonValueConverter<IReadOnlyList<Refund>>;
public class RefundListValueComparer : ValueComparer<IReadOnlyList<Refund>>
{
    public RefundListValueComparer()
        : base(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly()
        )
    { }
}

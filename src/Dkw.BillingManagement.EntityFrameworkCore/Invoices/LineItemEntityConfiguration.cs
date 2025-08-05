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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dkw.BillingManagement.Invoices;

public class LineItemEntityConfiguration : EntityConfiguration<LineItem>
{
    protected override String GetTableName() => "LineItems";

    public override void Configure(EntityTypeBuilder<LineItem> builder)
    {

        builder.Property(l => l.InvoiceId)
            .HasColumnName("InvoiceId")
            .IsRequired();

        builder.Property(q => q.AppliedTaxes)
            .HasColumnName("AppliedTaxes")
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasConversion<AppliedTaxListConverter>()
            .IsRequired()
            .Metadata.SetValueComparer(typeof(AppliedTaxListValueComparer));

        builder.Property(q => q.AppliedDiscounts)
            .HasColumnName("AppliedDiscounts")
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasConversion<DiscountListConverter>()
            .IsRequired()
            .Metadata.SetValueComparer(typeof(DiscountListValueComparer));

        builder.Property(l => l.Quantity)
            .HasColumnName("Quantity")
            .HasColumnType("DECIMAL(18,2)")
            .IsRequired();

        builder.Property(l => l.ItemInfo)
            .HasColumnName("ItemInfo")
            .HasConversion<ItemInfoConverter>()
            .IsRequired();

        base.Configure(builder);
    }
}

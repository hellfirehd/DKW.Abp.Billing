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

using Dkw.BillingManagement.Customers;
using Dkw.BillingManagement.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dkw.BillingManagement.Invoices;

public class InvoiceEntityConfiguration : EntityConfiguration<Invoice>
{
    protected override String GetTableName() => "Invoices";
    public override void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.Property(q => q.InvoiceNumber).HasColumnName("InvoiceNumber").HasMaxLength(BillingManagementConsts.MaxInvoiceNumberLength).IsRequired();
        builder.Property(q => q.InvoiceDate).HasColumnName("InvoiceDate").IsRequired();
        builder.Property(q => q.DueDate).HasColumnName("DueDate").IsRequired();
        builder.Property(q => q.Status).HasColumnName("Status").HasConversion<String>().IsRequired();
        builder.Property(q => q.CustomerId).HasColumnName("CustomerId").IsRequired();
        builder.Property(q => q.ReferenceNumber).HasColumnName("ReferenceNumber").HasMaxLength(BillingManagementConsts.MaxReferenceNumberLength).IsRequired();
        builder.Property(q => q.BillingAddress).HasColumnName("BillingAddress").HasConversion<AddressConverter>().IsRequired();
        builder.Property(q => q.ShippingAddress).HasColumnName("ShippingAddress").HasConversion<AddressConverter>().IsRequired();
        builder.Property(q => q.Shipping).HasColumnName("Shipping").HasConversion<ShippingInfoConverter>().IsRequired();

        builder.Property(q => q.LineItems)
            .HasColumnName("LineItems")
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasConversion<LineItemListValueConverter>()
            .IsRequired()
            .Metadata.SetValueComparer(typeof(LineItemListValueComparer));

        builder.Property(q => q.Discounts)
            .HasColumnName("Discounts")
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasConversion<DiscountListConverter>()
            .IsRequired()
            .Metadata.SetValueComparer(typeof(DiscountListValueComparer));

        builder.Property(q => q.Surcharges)
            .HasColumnName("Surcharges")
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasConversion<SurchargeListConverter>()
            .IsRequired()
            .Metadata.SetValueComparer(typeof(SurchargeValueComparer));

        builder.Property(q => q.ApplicableTaxes)
            .HasColumnName("ApplicableTaxes")
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasConversion<ApplicableTaxListValueConverter>()
            .IsRequired()
            .Metadata.SetValueComparer(typeof(ApplicableTaxListValueComparer));

        builder.Property(q => q.Refunds)
            .HasColumnName("Refunds")
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasConversion<RefundListConverter>()
            .IsRequired()
            .Metadata.SetValueComparer(typeof(RefundListValueComparer));

        //Relationships
        builder.HasOne(i => i.PlaceOfSupply)
            .WithMany()
            .HasForeignKey(i => i.PlaceOfSupplyId)
            .IsRequired();

        //Indexes
        builder.HasIndex(q => q.CreationTime);
        builder.HasIndex(q => q.CustomerId);

        base.Configure(builder);
    }
}

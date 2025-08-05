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

public class RefundEntityConfiguration : EntityConfiguration<Refund>
{
    protected override String GetTableName() => "Refunds";

    public override void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.Property(r => r.Amount)
            .HasColumnName("Amount")
            .HasColumnType("MONEY")
            .IsRequired();

        builder.Property(r => r.RefundDate)
            .HasColumnName("RefundDate")
            .HasColumnType("DATETIME2")
            .IsRequired();

        builder.Property(r => r.ReferenceNumber)
            .HasColumnName("ReferenceNumber")
            .HasMaxLength(BillingManagementConsts.MaxReferenceNumberLength)
            .IsRequired();

        builder.Property(r => r.Reason)
            .HasColumnName("Reason")
            .HasMaxLength(BillingManagementConsts.MaxReasonLength)
            .IsRequired();

        builder.Property(r => r.Notes)
            .HasColumnName("Notes")
            .IsRequired();

        builder.Property(r => r.IsShippingRefunded)
            .HasColumnName("IsShippingRefunded")
            .IsRequired();

        builder.Property(r => r.SubtotalRefund)
            .HasColumnName("SubtotalRefund")
            .HasColumnType("MONEY")
            .IsRequired();

        builder.Property(r => r.TaxRefund)
            .HasColumnName("TaxRefund")
            .HasColumnType("MONEY")
            .IsRequired();

        builder.Property(r => r.ShippingRefund)
            .HasColumnName("ShippingRefund")
            .HasColumnType("MONEY")
            .IsRequired();

        base.Configure(builder);
    }
}

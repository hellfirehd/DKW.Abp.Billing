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

namespace Dkw.BillingManagement.Taxes;

public class TaxEntityConfiguration : EntityConfiguration<Tax>
{
    protected override String GetTableName() => "Taxes";

    public override void Configure(EntityTypeBuilder<Tax> builder)
    {
        builder.Property(x => x.Name)
            .HasColumnName("Name")
            .HasMaxLength(BillingManagementConsts.MaxTaxNameLength)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("Code")
            .HasMaxLength(BillingManagementConsts.MaxTaxCodeLength)
            .IsRequired();

        //builder.OwnsMany(x => x.Rates, tr =>
        //{
        //    //tr.ToTable(BillingManagementDbProperties.DbTablePrefix + GetTableName(), BillingManagementDbProperties.DbSchema);

        //    ////tr.WithOwner().HasForeignKey(tr => tr.TaxId);

        //    //tr.Property(r => r.TaxId)
        //    //    .HasColumnName("TaxId")
        //    //    .IsRequired();

        //    //tr.Property(r => r.Rate)
        //    //    .HasColumnName("Rate")
        //    //    .HasPrecision(18, 4)
        //    //    .IsRequired();

        //    //tr.Property(r => r.IsActive)
        //    //    .HasColumnName("IsActive")
        //    //    .HasDefaultValue(true)
        //    //    .IsRequired();

        //    //tr.Property(r => r.EffectiveDate)
        //    //    .HasColumnName("EffectiveDate")
        //    //    .IsRequired();

        //    //tr.Property(r => r.ExpiryDate)
        //    //    .HasColumnName("ExpiryDate")
        //    //    .IsRequired(false);
        //});

        base.Configure(builder);
    }
}

public class TaxRateEntityConfiguration : EntityConfiguration<TaxRate>
{
    protected override String GetTableName() => "TaxRates";
    public override void Configure(EntityTypeBuilder<TaxRate> builder)
    {
        builder.Property(x => x.Rate)
            .HasColumnName("Rate")
            .HasPrecision(16, 6)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.EffectiveDate)
            .HasColumnName("EffectiveDate")
            .IsRequired();

        builder.Property(x => x.ExpiryDate)
            .HasColumnName("ExpiryDate")
            .IsRequired(false);

        base.Configure(builder);
    }
}

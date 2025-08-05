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
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dkw.BillingManagement.Provinces;

public class ProvinceEntityConfiguration : EntityConfiguration<Province>
{
    protected override String GetTableName() => "Provinces";

    public override void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.Property(x => x.Code).IsRequired().HasMaxLength(BillingManagementConsts.MaxProvinceCodeLength);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(BillingManagementConsts.MaxProvinceNameLength);
        builder.Property(x => x.HasHST).IsRequired();

        builder.HasMany(x => x.Taxes)
            .WithMany();

        base.Configure(builder);
    }
}

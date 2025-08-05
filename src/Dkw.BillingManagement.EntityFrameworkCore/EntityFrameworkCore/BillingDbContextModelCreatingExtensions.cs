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
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Dkw.BillingManagement.EntityFrameworkCore;

public static class BillingManagementDbContextModelCreatingExtensions
{
    public static void ConfigureBillingManagement(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        var assembly = typeof(BillingManagementDbContextModelCreatingExtensions).Assembly;
        builder.ApplyConfigurationsFromAssembly(assembly);

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureTenantManagement();

        builder.Entity<AppliedTax>(b =>
        {
            b.HasNoKey();
        });
    }
}

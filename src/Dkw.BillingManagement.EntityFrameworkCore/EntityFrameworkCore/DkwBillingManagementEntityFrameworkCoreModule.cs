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

using Dkw.Abp.EntityFrameworkCore;
using Dkw.BillingManagement.Provinces;
using Dkw.BillingManagement.Taxes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Dkw.BillingManagement.EntityFrameworkCore;

[DependsOn(typeof(AbpEntityFrameworkCoreModule))]
[DependsOn(typeof(AbpPermissionManagementEntityFrameworkCoreModule))]
[DependsOn(typeof(AbpSettingManagementEntityFrameworkCoreModule))]
[DependsOn(typeof(AbpTenantManagementEntityFrameworkCoreModule))]
[DependsOn(typeof(DkwBillingManagementDomainModule))]
[DependsOn(typeof(DkwEntityFrameworkCoreModule))]
public class DkwBillingManagementEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<BillingManagementDbContext>(options =>
        {
            options.AddDefaultRepositories<IBillingManagementDbContext>();
        });

        Configure<AbpEntityOptions>(options =>
        {
            options.Entity<Province>(province =>
            {
                province.DefaultWithDetailsFunc = query => query
                    .Include(o => o.Taxes)
                    .ThenInclude(t => t.Rates);
            });

            options.Entity<Tax>(tax =>
            {
                tax.DefaultWithDetailsFunc = query => query
                    .Include(o => o.Rates);
            });

            //options.Entity<ItemBase>(item =>
            //{
            //    item.DefaultWithDetailsFunc = query => query
            //        .Include(o => o.Pricing);
            //});
        });
    }
}

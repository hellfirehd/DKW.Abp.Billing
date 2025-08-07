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
using Dkw.BillingManagement.Items;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace Dkw.BillingManagement;

public class BillingManagementDataSeedContributor(
    ICurrentTenant currentTenant,
    IBillingManagementDbContext dbContext)
    : IDataSeedContributor, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IBillingManagementDbContext _dbContext = dbContext;

    private readonly DateOnly _date = DateOnly.FromDateTime(DateTime.UnixEpoch);

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            var province = await _dbContext.Provinces.Where(p => p.Code == "ON").SingleAsync();
            var customer = TestData.Customer(province, TestData.CustomerId);

            _dbContext.Customers.Add(customer);

            var taxableProduct = Product.Create(TestData.TaxableProductId, "TP-001", "A product that is taxable.", "STD-GOODS", itemCategory: ItemCategory.GeneralGoods).AddPrice(100.00m, _date);
            var nonTaxableProduct = Product.Create(TestData.NonTaxableProductId, "NTProduct-001", "A product that is not taxable.", "ZR-GROCERY", itemCategory: ItemCategory.BasicGroceries).AddPrice(50.00M, _date);
            var taxableService = Service.Create(TestData.TaxableServiceId, "Taxable Service", "A service that is taxable.", "STD-PROF", ItemCategory.FinancialServices).AddPrice(500.00m, _date);
            var nonTaxableService = Service.Create(TestData.NonTaxableServiceId, "Non-Taxable Service", "A service that is not taxable.", "EX-EDUCATION", ItemCategory.EducationalServices).AddPrice(75.00m, _date);

            _dbContext.ItemBase.AddRange([taxableProduct, nonTaxableProduct, taxableService, nonTaxableService]);

            await _dbContext.SaveChangesAsync();
        }
    }
}

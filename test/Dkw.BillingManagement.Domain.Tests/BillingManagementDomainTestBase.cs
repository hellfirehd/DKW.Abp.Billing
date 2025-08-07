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
using Dkw.BillingManagement.Invoices;
using Dkw.BillingManagement.Invoices.LineItems;
using Dkw.BillingManagement.Items;
using Dkw.BillingManagement.Provinces;
using Dkw.BillingManagement.Taxes;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Modularity;

namespace Dkw.BillingManagement;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */

public abstract class BillingManagementDomainTestBase<TStartupModule, TSut> : BillingManagementDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
    where TSut : class
{
    protected TSut SUT { get; }

    protected override void BeforeAddApplication(IServiceCollection services)
        => services.AddTransient<TSut>();

    protected BillingManagementDomainTestBase()
    {
        SUT = GetRequiredService<TSut>();
    }
}

public abstract class BillingManagementDomainTestBase<TStartupModule> : BillingManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected ICustomerRepository CustomerRepository { get; }
    protected IGuidGenerator GuidGenerator { get; }
    protected IItemRepository ItemRepository { get; }
    protected IInvoiceManager InvoiceManager { get; }
    protected ILineItemFactory LineItemFactory { get; }
    protected IProvinceRepository ProvinceRepository { get; }
    protected ITaxManager TaxProvider { get; }

    protected BillingManagementDomainTestBase()
    {
        CustomerRepository = GetRequiredService<ICustomerRepository>();
        GuidGenerator = GetRequiredService<IGuidGenerator>();
        InvoiceManager = GetRequiredService<IInvoiceManager>();
        ItemRepository = GetRequiredService<IItemRepository>();
        LineItemFactory = GetRequiredService<ILineItemFactory>();
        ProvinceRepository = GetRequiredService<IProvinceRepository>();
        TaxProvider = GetRequiredService<ITaxManager>();
    }

    public async Task<LineItem> CreateLineItem(Guid itemId, DateOnly effectiveDate, Decimal quantity = 1.0m)
    {
        var item = await ItemRepository.GetAsync(itemId);

        return await LineItemFactory.CreateAsync(item, effectiveDate);
    }

    /// <summary>
    /// Creates a taxable <see cref="ItemCategory.BooksAndMagazines"/> product worth $100.00.
    /// </summary>
    public async Task<LineItem> TaxableProductItemAsync(DateOnly effectiveDate)
    {
        return await CreateLineItem(TestData.TaxableProductId, effectiveDate);
    }

    /// <summary>
    /// Creates a non-taxable <see cref="ItemCategory.BasicGroceries"/> product worth $50.00.
    /// </summary>
    public async Task<LineItem> NonTaxableProductItemAsync(DateOnly effectiveDate)
    {
        return await CreateLineItem(TestData.NonTaxableProductId, effectiveDate);
    }

    /// <summary>
    /// Creates a taxable <see cref="ItemCategory.ProfessionalServices"/> service worth $500.00.
    /// </summary>
    public async Task<LineItem> TaxableServiceItemAsync(DateOnly effectiveDate)
    {
        return await CreateLineItem(TestData.TaxableServiceId, effectiveDate);
    }

    /// <summary>
    /// Creates a non-taxable <see cref="ItemCategory.EducationalServices"/> service worth $75.00.
    /// </summary>
    public Task<LineItem> NonTaxableServiceItemAsync(DateOnly effectiveDate)
    {
        return CreateLineItem(TestData.NonTaxableServiceId, effectiveDate);
    }
}

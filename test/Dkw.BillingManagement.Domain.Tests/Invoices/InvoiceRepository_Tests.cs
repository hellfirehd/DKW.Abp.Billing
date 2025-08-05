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

using Dkw.BillingManagement.Provinces;
using Shouldly;
using Volo.Abp.Guids;
using Volo.Abp.Modularity;

namespace Dkw.BillingManagement.Invoices;

/* Write your custom repository tests like that, in this project, as abstract classes.
 * Then inherit these abstract classes from EF Core & MongoDB test projects.
 * In this way, both database providers are tests with the same set tests.
 */
public abstract class InvoiceRepository_Tests<TStartupModule> : BillingManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly DateOnly EffectiveDate = new(2020, 01, 01);

    protected IGuidGenerator GuidGenerator { get; }
    protected IProvinceRepository ProvinceRepository { get; }

    private IInvoiceRepository SUT { get; }

    protected InvoiceRepository_Tests()
    {
        GuidGenerator = GetRequiredService<IGuidGenerator>();
        ProvinceRepository = GetRequiredService<IProvinceRepository>();

        SUT = GetRequiredService<IInvoiceRepository>();
    }

    [Fact]
    public async Task InvoiceRepository_InsertInvoice_Succeeds()
    {
        var province = await ProvinceRepository.GetAsync("BC");

        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(province), province.Id, EffectiveDate);

        var result = await SUT.InsertAsync(invoice);

        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Id.ShouldBe(invoice.Id);
    }
}

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

using Shouldly;
using Volo.Abp.Modularity;

namespace Dkw.BillingManagement.Invoices;

/* Write your custom repository tests like that, in this project, as abstract classes.
 * Then inherit these abstract classes from EF Core & MongoDB test projects.
 * In this way, both database providers are tests with the same set tests.
 */
public abstract class InvoiceRepository_Tests<TStartupModule> : BillingManagementDomainTestBase<TStartupModule, InvoiceRepository>
    where TStartupModule : IAbpModule
{
    private readonly DateOnly EffectiveDate = new(2020, 01, 01);

    [Fact]
    public async Task InvoiceRepository_InsertInvoice_Succeeds()
    {
        var customer = await CustomerRepository.GetAsync(TestData.CustomerId);
        var province = await ProvinceRepository.GetAsync(customer.ShippingAddress.Province);
        var invoiceId = GuidGenerator.Create();

        var invoice = Invoice.Create(invoiceId, customer.Id, province.Id, customer.ShippingAddress, customer.BillingAddress, EffectiveDate);

        var result = await SUT.InsertAsync(invoice);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(invoiceId);
        result.InvoiceDate.ShouldBe(EffectiveDate);
        result.CustomerId.ShouldBe(customer.Id);
        result.PlaceOfSupplyId.ShouldBe(province.Id);
        result.BillingAddress.ShouldBe(customer.BillingAddress);
        result.ShippingAddress.ShouldBe(customer.ShippingAddress);
        result.Status.ShouldBe(InvoiceStatus.Draft);
    }
}

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

using Dkw.BillingManagement.Taxes;
using Shouldly;
using Volo.Abp.Modularity;

namespace Dkw.BillingManagement.Invoices;

public abstract class InvoiceManager_IntegrationTests<TStartupModule> : BillingManagementDomainTestBase<TStartupModule, InvoiceManager>
    where TStartupModule : IAbpModule
{
    [Fact]
    public async Task InvoiceManager_CreateInvoice_ShouldSucceed()
    {
        // Arrange
        var customer = await CustomerRepository.GetAsync(TestData.CustomerId);

        // Act
        var invoiceId = await SUT.CreateInvoiceAsync(customer, []);

        // Assert
        invoiceId.ShouldNotBe(Guid.Empty);
        var created = await SUT.GetInvoiceAsync(invoiceId);
        created.ShouldNotBeNull();
    }

    [Fact]
    public async Task InvoiceManager_ApplyTaxes_ShouldApplyTaxes_OnlyOnTaxableItems()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.Now);
        var province = await ProvinceRepository.GetAsync("BC");
        var customer = TestData.Customer(province);
        var profile = new CustomerTaxProfile();
        var invoice = new Invoice(GuidGenerator.Create(), customer, province, date);

        var taxableProduct = await TaxableProductItemAsync(date); // $100
        invoice.AddLineItem(taxableProduct);

        var nonTaxableProduct = await NonTaxableProductItemAsync(date); // $50
        invoice.AddLineItem(nonTaxableProduct);

        // Act
        await SUT.ApplyTaxesAsync(invoice);

        // Assert
        Assert.Equal(14.00m, invoice.GetTaxAmount()); // 100 * 0.14 = 14.00
        Assert.Single(taxableProduct.AppliedTaxes);
        Assert.Empty(nonTaxableProduct.AppliedTaxes);
    }
}

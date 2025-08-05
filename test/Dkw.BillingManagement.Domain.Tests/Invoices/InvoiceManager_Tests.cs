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
using Dkw.BillingManagement.Provinces;
using Shouldly;
using Volo.Abp.Modularity;

namespace Dkw.BillingManagement.Invoices;

public abstract class InvoiceManager_Tests<TStartupModule> : BillingManagemenDomainTestBase<TStartupModule>, IAsyncLifetime
    where TStartupModule : IAbpModule
{
    private readonly DateOnly _effectiveDate = TestData.EffectiveDate;

    private Customer _customer = default!;
    private Province _placeOfSupply = default!;
    private Guid _placeOfSupplyId;

    private readonly InvoiceManager SUT;

    public InvoiceManager_Tests()
    {
        SUT = GetRequiredService<InvoiceManager>();
    }

    public async Task InitializeAsync()
    {
        _customer = TestData.Customer();
        _placeOfSupply = await ProvinceRepository.GetAsync(_customer.ShippingAddress.Province);
        _placeOfSupplyId = _placeOfSupply.Id;
    }

    public Task DisposeAsync() => throw new NotImplementedException();

    [Fact]
    public async Task InvoiceManager_CreateInvoice_ShouldSucceed()
    {
        // Arrange
        var invoiceId = GuidGenerator.Create();
        var invoice = new Invoice(invoiceId, _customer, _placeOfSupplyId, _effectiveDate);

        // Act
        var result = await SUT.CreateInvoiceAsync(invoice);

        // Assert
        result.ShouldBe(invoiceId);
        var created = await SUT.GetInvoiceAsync(invoiceId);
        created.ShouldNotBeNull();
    }
}

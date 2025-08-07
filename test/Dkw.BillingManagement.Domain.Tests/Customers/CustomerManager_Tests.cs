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

namespace Dkw.BillingManagement.Customers;

public abstract class CustomerManager_Tests<TStartupModule> : BillingManagementDomainTestBase<TStartupModule, CustomerManager>
    where TStartupModule : IAbpModule
{
    [Fact]
    public async Task CustomerManager_GetCustomer_ShouldLoadAddresses()
    {
        // Arrange

        // Act
        var customer = await SUT.GetCustomerAsync(TestData.CustomerId);

        Assert.NotNull(customer);
        customer.Addresses.Count.ShouldBe(2);
        customer.BillingAddress.ShouldNotBeNull();
        customer.BillingAddress.Province.ShouldBe("ON");
        customer.ShippingAddress.ShouldNotBeNull();
    }
}

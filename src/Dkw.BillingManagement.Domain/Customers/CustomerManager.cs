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
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace Dkw.BillingManagement.Customers;

[ExposeServices(typeof(ICustomerManager))]
public class CustomerManager(ICustomerRepository customerRepository) : DomainService, ICustomerManager, ITransientDependency
{
    private readonly ICustomerRepository _customerRepository = customerRepository;

    public async Task<Customer> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetAsync(customerId, includeDetails: true, cancellationToken);

        return customer;
    }

    public Task<CustomerTaxProfile> GetTaxProfileAsync(Guid customerId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}

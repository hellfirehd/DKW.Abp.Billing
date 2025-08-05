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

using Dkw.BillingManagement.Payments;
using Dkw.BillingManagement.Shipping;
using Volo.Abp.Domain.Entities;

namespace Dkw.BillingManagement.Customers;

public class Customer : Entity<Guid>
{
    private readonly List<Address> _addresses = [];
    private readonly List<PaymentMethod> _paymentMethods = [];

    protected Customer()
    {
        // Parameterless constructor for EF Core/JSON serialization
    }

    public Customer(String name, Email email, IReadOnlyList<Address> addresses, CustomerType customerType = CustomerType.Regular, CustomerTaxType recipientStatus = CustomerTaxType.Regular)
    {
        Name = name;
        Email = email;
        CustomerType = customerType;
        RecipientStatus = recipientStatus;

        _addresses.AddRange(addresses);
    }

    public Customer SetId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ID cannot be empty.", nameof(id));
        }

        Id = id;

        return this;
    }

    public virtual String Name { get; protected set; } = String.Empty;
    public virtual Email Email { get; protected set; } = Email.Empty;
    public virtual IReadOnlyList<Address> Addresses => _addresses;
    public virtual IReadOnlyList<PaymentMethod> PaymentMethods => _paymentMethods;
    public virtual CustomerType CustomerType { get; protected set; } = CustomerType.Regular;
    public virtual CustomerTaxType RecipientStatus { get; protected set; } = CustomerTaxType.Regular;

    public virtual Address ShippingAddress
        => Addresses.FirstOrDefault(a => a.IsShippingAddress)
        ?? Addresses.FirstOrDefault(a => a.IsDefault)
        ?? throw new BillingManagementException(ErrorCodes.NotFound, "This customer does not have a shipping address or default address.");

    public virtual Address BillingAddress
        => Addresses.FirstOrDefault(a => a.IsBillingAddress)
        ?? Addresses.FirstOrDefault(a => a.IsDefault)
        ?? throw new BillingManagementException(ErrorCodes.NotFound, "This customer does not have a billing address or default address.");

    public virtual ShippingInfo PreferredShipping { get; protected set; } = ShippingInfo.Empty;

    public void AddAddress(Address address)
    {
        _addresses.Add(address);
    }

    public void RemoveAddress(Address address)
    {
        _addresses.Remove(address);
    }
}

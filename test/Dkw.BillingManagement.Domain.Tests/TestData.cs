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

using Bogus;
using Dkw.BillingManagement.Customers;
using Dkw.BillingManagement.Invoices;
using Dkw.BillingManagement.Items;
using Dkw.BillingManagement.Payments;
using Dkw.BillingManagement.Provinces;

namespace Dkw.BillingManagement;

/// <summary>
/// Builder class for creating test data for billing system tests
/// </summary>
public static class TestData

{
    public static readonly DateOnly EffectiveDate = new(2020, 01, 01);

    public static readonly Guid DefaultCustomerId = Guid.NewGuid();

    /// <summary>
    /// Represents the unique identifier for a taxable product worth $100.00.
    /// </summary>
    public static readonly Guid TaxableProductId = Guid.NewGuid();

    /// <summary>
    /// Represents the unique identifier for a non-taxable <see cref="ItemCategory.BasicGroceries"/> product worth $50.00.
    /// </summary>
    public static readonly Guid NonTaxableProductId = Guid.NewGuid();

    /// <summary>
    /// Represents the unique identifier for a taxable service worth $500.00.
    /// </summary>
    public static readonly Guid TaxableServiceId = Guid.NewGuid();

    /// <summary>
    /// Represents the unique identifier for a taxable service worth $75.00.
    /// </summary>
    public static readonly Guid NonTaxableServiceId = Guid.NewGuid();

    public static readonly String[] Provinces = ["AB", "BC", "MB", "NB", "NL", "NS", "NT", "NU", "ON", "PE", "QC", "SK", "YT"];

    public static PaymentMethod CreditCard() => new CreditCardPaymentMethod
    {
        CardNumber = "411111111111",
        ExpiryDate = "12/25",
        CardHolderName = "John Doe",
        Cvv = "123"
    };

    public static PaymentMethod BankTransfer() => new BankTransferPaymentMethod();

    public static PaymentMethod Cash() => new CashPaymentMethod();

    public static Customer Customer(Province? province = null, Guid? customerId = null)
    {
        var f = new Faker();

        return new Customer(f.Person.FullName, f.Person.Email, [Address(province, isDefault: false, isBilling: true), Address(province, isDefault: false, isShipping: true)])
            .SetId(customerId ?? Guid.NewGuid());
    }

    public static Address Address(Province? province = null, Boolean isDefault = false, Boolean? isBilling = null, Boolean? isShipping = null)
    {
        var f = new Faker();

        return new Address(
            f.Person.FullName,
            f.Address.StreetAddress(),
            String.Empty, f.Address.City(),
            province is null ? f.PickRandom(Provinces) : province,
            f.Address.ZipCode(),
            "Canada",
            new PhoneNumber(f.Person.Phone),
            isDefault,
            isDefault || (isShipping ?? f.Random.Bool()),
            isDefault || (isShipping ?? f.Random.Bool()));
    }

    /// <summary>
    /// Creates a simple invoice for basic testing
    /// </summary>
    public static Invoice EmptyInvoice(Province placeOfSupply)
    {
        var customer = Customer(placeOfSupply);

        return new Invoice(Guid.NewGuid(), customer, placeOfSupply.Id, EffectiveDate);
    }

    public static ItemBase Item(Guid id, Decimal unitPrice = 50.00m, ItemType itemType = ItemType.Product, ItemCategory itemCategory = ItemCategory.BasicGroceries)
    {
        return MockItem.Create(id, itemType, itemCategory)
            .AddPrice(unitPrice, DateOnly.FromDateTime(DateTime.UnixEpoch));
    }

    // Helper class for testing
    private sealed class MockItem : ItemBase<MockItem>
    {
        public override ItemType ItemType { get; protected set; }

        public static MockItem Create(Guid id, ItemType itemType, ItemCategory itemCategory)
        {
            return new MockItem
            {
                Id = id,
                SKU = String.Empty,
                Name = String.Empty,
                ItemType = itemType,
                ItemCategory = itemCategory
            };
        }
    }
}

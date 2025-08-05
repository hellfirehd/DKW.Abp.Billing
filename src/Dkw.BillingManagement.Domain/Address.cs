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
using Volo.Abp.Domain.Values;

namespace Dkw.BillingManagement;

public class Address(
    String name,
    String line1,
    String line2,
    String city,
    String province,
    String postalCode,
    String country,
    PhoneNumber phoneNumber,
    Boolean isDefault = false,
    Boolean isShippingAddress = false,
    Boolean isBillingAddress = false) : ValueObject
{
    public static readonly Address Empty = new();

    // Required for EF Core
    protected Address()
        : this(
            String.Empty,
            String.Empty,
            String.Empty,
            String.Empty,
            String.Empty,
            String.Empty,
            String.Empty,
            PhoneNumber.Empty
        )
    {
    }

    public String Name { get; } = name;
    public String Line1 { get; } = line1;
    public String Line2 { get; } = line2;
    public String City { get; } = city;
    public String Province { get; } = province;
    public String PostalCode { get; } = postalCode;
    public String Country { get; } = country;
    public PhoneNumber PhoneNumber { get; } = phoneNumber ?? PhoneNumber.Empty;
    public Boolean IsDefault { get; } = isDefault;
    public Boolean IsShippingAddress { get; } = isShippingAddress;
    public Boolean IsBillingAddress { get; } = isBillingAddress;

    protected override IEnumerable<Object> GetAtomicValues()
    {
        yield return Name;
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return Province;
        yield return PostalCode;
        yield return Country;
        yield return PhoneNumber;
        yield return IsDefault;
        yield return IsShippingAddress;
        yield return IsBillingAddress;
    }
}

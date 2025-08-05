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

using Dkw.BillingManagement.EntityFrameworkCore;
using Dkw.BillingManagement.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dkw.BillingManagement.Customers;

public class CustomerEntityConfiguration : EntityConfiguration<Customer>
{
    protected override String GetTableName() => "Customers";

    public override void Configure(EntityTypeBuilder<Customer> builder)
    {

        builder.Property(c => c.Name).HasColumnName("Name").HasMaxLength(256).IsRequired();

        //builder.OwnsOne(c => c.Email, n => n.ToJson());

        builder.Property(q => q.Addresses)
            .HasField("_addresses")
            .HasColumnName("Addresses")
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasConversion<AddressListConverter>()
            .IsRequired()
            .Metadata.SetValueComparer(typeof(AddressListValueComparer));

        builder.Property(c => c.PaymentMethods)
            .HasField("_paymentMethods")
            .HasColumnName("PaymentMethods")
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasConversion<PaymentMethodListConverter>()
            .IsRequired()
            .Metadata.SetValueComparer(typeof(PaymentMethodListValueComparer));

        base.Configure(builder);
    }
}

public class AddressConverter : JsonValueConverter<Address>;

public class AddressListConverter : JsonValueConverter<IReadOnlyList<Address>>;

public class AddressListValueComparer : ValueComparer<IReadOnlyList<Address>>
{
    public AddressListValueComparer()
        : base(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly()
        )
    { }
}

public class PaymentMethodListConverter : JsonValueConverter<IReadOnlyList<PaymentMethod>>;
public class PaymentMethodListValueComparer : ValueComparer<IReadOnlyList<PaymentMethod>>
{
    public PaymentMethodListValueComparer()
        : base(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly()
        )
    { }
}

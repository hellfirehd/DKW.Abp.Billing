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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dkw.BillingManagement.Items;

public class ItemBaseEntityConfigurationBase : EntityConfiguration<ItemBase>
{
    protected override String GetTableName() => "Items";
    protected override String GetPrimaryKeyName() => "ItemId";

    public override void Configure(EntityTypeBuilder<ItemBase> builder)
    {
        builder.HasDiscriminator(i => i.ItemType)
            .HasValue<Product>(ItemType.Product)
            .HasValue<Service>(ItemType.Service)
            .IsComplete(false);

        builder.Property(i => i.Name)
            .HasColumnName("Name")
            .HasMaxLength(BillingManagementConsts.MaxNameLength)
            .IsRequired();

        builder.Property(i => i.Description)
            .HasColumnName("Description")
            .HasMaxLength(BillingManagementConsts.MaxDescriptionLength)
            .IsRequired();

        builder.Property(i => i.TaxCode)
            .HasColumnName("TaxCode")
            .HasMaxLength(BillingManagementConsts.MaxTaxCodeLength)
            .IsRequired();

        builder.Property(i => i.UnitType)
            .HasColumnName("UnitType")
            .HasMaxLength(BillingManagementConsts.MaxUnitTypeLength)
            .IsRequired();

        builder.Property(i => i.ItemType)
            .HasColumnName("ItemType")
            .IsRequired();

        builder.Property(i => i.ItemCategory)
            .HasColumnName("ItemCategory")
            .IsRequired();

        builder.Property(i => i.IsActive)
            .HasColumnName("IsActive")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(q => q.Pricing)
            .HasField("_pricing")
            .HasColumnName("Pricing")
            .UsePropertyAccessMode(PropertyAccessMode.PreferField)
            .HasConversion<ItemPriceListConverter>()
            .IsRequired()
            .Metadata.SetValueComparer(typeof(ItemPriceListValueComparer));

        base.Configure(builder);
    }
}

public class ItemPriceConverter : JsonValueConverter<ItemPrice>;

public class ItemPriceListConverter : JsonValueConverter<IReadOnlyCollection<ItemPrice>>;

public class ItemPriceListValueComparer : ValueComparer<IReadOnlyCollection<ItemPrice>>
{
    public ItemPriceListValueComparer()
        : base(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList().AsReadOnly()
        )
    { }
}

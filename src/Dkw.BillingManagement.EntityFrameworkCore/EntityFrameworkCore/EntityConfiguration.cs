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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using Volo.Abp.Domain.Entities;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Dkw.BillingManagement.EntityFrameworkCore;

public abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IEntity<Guid>
{
    protected virtual JsonSerializerOptions JsonSerializerOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    protected abstract string GetTableName();

    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        var name = typeof(TEntity).Name;

        builder.ToTable(BillingManagementDbProperties.DbTablePrefix + GetTableName(), BillingManagementDbProperties.DbSchema);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName(name + "Id").ValueGeneratedOnAdd();

        if (typeof(TEntity).GetInterfaces().Contains(typeof(IHasCorrelationId)))
        {
            builder.Property(nameof(IHasCorrelationId.CorrelationId)).HasColumnName("CorrelationId").ValueGeneratedOnAdd().IsRequired(true);
            builder.HasIndex(nameof(IHasCorrelationId.CorrelationId)).HasAnnotation("SqlServer:Clustered", false).IsUnique(true);
        }

        builder.ConfigureByConvention();
    }
}

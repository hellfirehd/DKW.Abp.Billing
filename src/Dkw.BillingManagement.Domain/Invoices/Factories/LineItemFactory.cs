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

using Dkw.BillingManagement.Items;
using Volo.Abp.DependencyInjection;

namespace Dkw.BillingManagement.Invoices.Factories;

[ExposeServices(typeof(ILineItemFactory))]
public class LineItemFactory(IEnumerable<IInvoiceLineItemFactory> factories)
    : ILineItemFactory, ITransientDependency
{
    private readonly IEnumerable<IInvoiceLineItemFactory> _factories = factories
        ?? throw new InvalidOperationException($"No {nameof(IInvoiceLineItemFactory)} have been registered.");

    public Boolean CanCreate(ItemBase item) => _factories.Any(factory => factory.CanCreate(item));

    public async Task<LineItem> CreateAsync(Guid id, ItemBase item, DateOnly invoiceDate, CancellationToken cancellationToken = default)
    {
        var factory = _factories.FirstOrDefault(f => f.CanCreate(item))
            ?? throw new ArgumentException($"No factory found for item type {item.ItemType}.", nameof(item));

        return await factory.CreateAsync(id, item, invoiceDate, cancellationToken);
    }
}

public abstract class LineItemFactory<T> : IInvoiceLineItemFactory, ITransientDependency
    where T : ItemBase<T>
{
    public Boolean CanCreate(ItemBase item) => item is T;

    public Task<LineItem> CreateAsync(Guid id, ItemBase item, DateOnly invoiceDate, CancellationToken cancellationToken = default)
    {
        if (!CanCreate(item))
        {
            throw new ArgumentException($"{nameof(ItemBase.ItemType)} must be of type {ItemType.Product}.", nameof(item));
        }

        if (!item.IsAvailableOn(invoiceDate))
        {
            throw new ArgumentOutOfRangeException(nameof(invoiceDate),
                "Effective date must be on or after the earliest price effective date.");
        }

        var invoiceItem = new LineItem(id)
        {
            InvoiceDate = invoiceDate,
            SortOrder = 0, // Default sort order, can be adjusted later
            ItemInfo = new ItemInfo
            {
                ItemId = item.Id,
                SKU = item.SKU,
                Name = item.Name,
                Description = item.Description,
                UnitPrice = item.GetUnitPrice(invoiceDate),
                UnitType = item.UnitType,
                ItemType = item.ItemType,
                ItemCategory = item.ItemCategory,
                TaxCode = item.TaxCode
            },
        };

        SetProperties((T)item, ref invoiceItem);

        return Task.FromResult(invoiceItem);
    }

    // ToDo: This needs a better name.
    protected virtual Task SetProperties(T item, ref LineItem invoiceItem) => Task.CompletedTask;
}

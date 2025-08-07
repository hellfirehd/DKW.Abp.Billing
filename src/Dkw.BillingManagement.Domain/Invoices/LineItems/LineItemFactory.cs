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
using Volo.Abp.Guids;

namespace Dkw.BillingManagement.Invoices.LineItems;

[ExposeServices(typeof(ILineItemFactory))]
public class LineItemFactory(ILineItemFactoryProvider manager, IGuidGenerator guidGenerator)
        : ILineItemFactory, ITransientDependency
{
    public ILineItemFactoryProvider Manager { get; } = manager;
    public IGuidGenerator GuidGenerator { get; } = guidGenerator;

    public Boolean CanCreate(ItemBase item) => Manager.Factories.Any(factory => factory.CanCreate(item));

    public async Task<LineItem> CreateAsync(ItemBase item, DateOnly invoiceDate, CancellationToken cancellationToken = default)
    {
        var factory = Manager.Factories.FirstOrDefault(f => f.CanCreate(item))
            ?? throw new InvalidOperationException($"No factory found for item type {item.ItemType}.");

        var lineItemId = GuidGenerator.Create();

        return await factory.CreateAsync(lineItemId, item, invoiceDate, cancellationToken);
    }
}

[ExposeServices(typeof(IInvoiceLineItemFactory))]
public abstract class LineItemFactory<T> : IInvoiceLineItemFactory, ITransientDependency
    where T : ItemBase
{
    public Boolean CanCreate(ItemBase item) => item is T;

    protected abstract ItemType ItemType { get; }
    public Task<LineItem> CreateAsync(Guid id, ItemBase item, DateOnly invoiceDate, CancellationToken cancellationToken = default)
    {
        if (!CanCreate(item))
        {
            throw new ArgumentException($"{nameof(Item.ItemType)} must be of type {ItemType}.", nameof(item));
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

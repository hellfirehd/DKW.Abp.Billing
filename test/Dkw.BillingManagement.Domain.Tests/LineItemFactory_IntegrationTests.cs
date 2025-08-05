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

using Dkw.BillingManagement.Invoices;
using Dkw.BillingManagement.Invoices.Factories;
using Dkw.BillingManagement.Items;
using Volo.Abp.Modularity;

namespace Dkw.BillingManagement;

public class LineItemFactory_IntegrationTests<TStartupModule> : BillingManagemenDomainTestBase<TStartupModule, LineItemFactory>
    where TStartupModule : IAbpModule
{
    private readonly DateOnly _testDate = new(2023, 1, 1);

    private readonly Guid _testItemId = Guid.NewGuid();
    private readonly ItemBase _testItem;

    private readonly Guid _testLineItemId = Guid.NewGuid();
    private readonly LineItem _testLineItem;

    public LineItemFactory_IntegrationTests()
    {
        // Setup test data
        _testItem = Product.Create(_testItemId, "TEST001", "Test Item", "ZR-GROCERY", itemCategory: ItemCategory.BasicGroceries);

        _testLineItem = new LineItem(_testLineItemId)
        {
            ItemInfo = new ItemInfo
            {
                ItemId = _testItem.Id,
                SKU = _testItem.SKU,
                Name = _testItem.Name,
                Description = _testItem.Description,
                UnitPrice = _testItem.GetUnitPrice(_testDate),
                UnitType = _testItem.UnitType,
                ItemType = _testItem.ItemType,
                ItemCategory = _testItem.ItemCategory,
                TaxCode = _testItem.TaxCode
            },
            InvoiceDate = _testDate
        };
    }

    [Fact]
    public async Task Create_WithValidItemAndFactory_ReturnsLineItem()
    {
        // Arrange
        var lineItemId = Guid.NewGuid();
        var mockItem = new MockItem(lineItemId, "TEST001", "Test Item", "Description", ItemType.Product, ItemCategory.GeneralGoods, "STD-GOODS");

        // Act
        var result = await SUT.CreateAsync(lineItemId, mockItem, _testDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lineItemId, result.ItemInfo.ItemId);
    }

    [Fact]
    public async Task Create_WithNoMatchingFactory_ThrowsInvalidOperationException()
    {
        // Arrange
        var factory = new LineItemFactory([]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async ()
            => await SUT.CreateAsync(Guid.NewGuid(), _testItem, _testDate));
    }

    [Fact]
    public async Task Create_WithMultipleFactories_SelectsCorrectFactory()
    {
        // Arrange
        var mockFactory1 = new Mock<IInvoiceLineItemFactory>();
        var mockFactory2 = new Mock<IInvoiceLineItemFactory>();

        mockFactory1.Setup(f => f.CanCreate(_testItem)).Returns(false);
        mockFactory2.Setup(f => f.CanCreate(_testItem)).Returns(true);
        mockFactory2.Setup(f => f.CreateAsync(It.IsAny<Guid>(), _testItem, _testDate, It.IsAny<CancellationToken>())).ReturnsAsync(_testLineItem);

        var factory = new LineItemFactory([mockFactory1.Object, mockFactory2.Object]);

        // Act
        var result = await factory.CreateAsync(Guid.NewGuid(), _testItem, _testDate);

        // Assert
        Assert.NotNull(result);
        mockFactory1.Verify(f => f.CreateAsync(It.IsAny<Guid>(), It.IsAny<ItemBase>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        mockFactory2.Verify(f => f.CreateAsync(It.IsAny<Guid>(), It.IsAny<ItemBase>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LineItem_ShouldSetTheItemType_TheSameAsTheOriginalItem()
    {
        // Arrange
        var item = new MockItem(Guid.NewGuid(), "TEST002", "Test Item 2", "Description",
            ItemType.Product, ItemCategory.GeneralGoods, "STD-GOODS");

        // Act
        var lineItem = await SUT.CreateAsync(Guid.NewGuid(), item, _testDate);

        // Assert
        Assert.Equal(item.ItemType, lineItem.ItemInfo.ItemType);
    }

    // Helper class for testing
    private sealed class MockItem(
        Guid id,
        String sku,
        String name,
        String description,
        ItemType itemType,
        ItemCategory itemCategory,
        String taxCode)
        : ItemBase(id, sku, name, description, itemType, itemCategory, taxCode)
    {
        public override ItemType ItemType { get; protected set; }
    }
}

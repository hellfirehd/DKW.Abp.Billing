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

public class LineItemFactory_Tests<TStartupModule> : BillingManagemenDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly Mock<IInvoiceLineItemFactory> _mockItemFactory;

    private readonly Guid _itemId = Guid.NewGuid();
    private readonly ItemBase _testItem;

    private readonly Guid _lineItemId = Guid.NewGuid();
    private readonly LineItem _lineItem;

    private readonly DateOnly _testDate = new(2023, 1, 1);
    private readonly Decimal _testQuantity = 5.0m;

    private LineItemFactory SUT { get; }

    public LineItemFactory_Tests()
    {
        // Setup test data
        _testItem = TestData.Item(_itemId);

        _lineItem = new LineItem(Guid.NewGuid())
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
            InvoiceDate = _testDate,
            Quantity = _testQuantity
        };

        // Setup mocks
        _mockItemFactory = new Mock<IInvoiceLineItemFactory>();

        // Factory setup
        _mockItemFactory.Setup(f => f.CanCreate(_testItem)).Returns(true);
        _mockItemFactory.Setup(f => f.CreateAsync(_lineItemId, _testItem, _testDate, It.IsAny<CancellationToken>())).ReturnsAsync(_lineItem);

        // Create manager with mocks
        SUT = new LineItemFactory([_mockItemFactory.Object]);
    }

    [Fact]
    public async Task Create_WithValidItemAndFactory_ReturnsLineItem()
    {
        // Act
        var result = await SUT.CreateAsync(_itemId, _testItem, _testDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_itemId, result.ItemInfo.ItemId);
        Assert.Equal(_testQuantity, result.Quantity);

        // Verify factory was called
        _mockItemFactory.Verify(f => f.CreateAsync(_lineItemId, _testItem, _testDate, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithInvalidItemId_ThrowsArgumentException()
    {
        // Arrange
        var invalidItemId = Guid.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await SUT.CreateAsync(invalidItemId, _testItem, _testDate));
    }

    [Fact]
    public async Task Create_WithNoMatchingFactory_ThrowsInvalidOperationException()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = TestData.Item(itemId);

        _mockItemFactory.Setup(f => f.CanCreate(item)).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await SUT.CreateAsync(itemId, item, _testDate));
    }

    [Fact]
    public async Task CreateAsync_WithMultipleFactories_SelectsCorrectFactory()
    {
        // Arrange
        var mockFactory1 = new Mock<IInvoiceLineItemFactory>();
        var mockFactory2 = new Mock<IInvoiceLineItemFactory>();

        mockFactory1.Setup(f => f.CanCreate(_testItem)).Returns(false);
        mockFactory2.Setup(f => f.CanCreate(_testItem)).Returns(true);
        mockFactory2.Setup(f => f.CreateAsync(_lineItemId, _testItem, _testDate, It.IsAny<CancellationToken>())).ReturnsAsync(_lineItem);

        var factory = new LineItemFactory([mockFactory1.Object, mockFactory2.Object]);

        // Act
        var result = await factory.CreateAsync(_lineItemId, _testItem, _testDate);

        // Assert
        Assert.NotNull(result);
        mockFactory1.Verify(f => f.CreateAsync(It.IsAny<Guid>(), It.IsAny<ItemBase>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        mockFactory2.Verify(f => f.CreateAsync(It.IsAny<Guid>(), It.IsAny<ItemBase>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

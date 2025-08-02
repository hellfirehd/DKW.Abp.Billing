// DKW ABP Framework Extensions
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

namespace Dkw.Abp.Billing;

public class InvoiceItemManagerTests
{
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly Mock<IInvoiceItemFactory> _mockItemFactory;
    private readonly Guid _validItemId = Guid.NewGuid();
    private readonly DateOnly _testDate = new(2023, 1, 1);
    private readonly decimal _testQuantity = 5.0m;
    private readonly Item _testItem;
    private readonly InvoiceItem _testInvoiceItem;

    private InvoiceItemManager SUT { get; }

    public InvoiceItemManagerTests()
    {
        // Setup test data
        _testItem = new MockItem(_validItemId, "TEST001", "Test Item", "Description",
            ItemType.Product, ItemCategory.GeneralGoods, new TaxCode());

        _testInvoiceItem = new InvoiceItem
        {
            Id = Guid.NewGuid(),
            ItemId = _validItemId,
            SKU = "TEST001",
            Name = "Test Item",
            UnitPrice = 10.0m,
            InvoiceDate = _testDate
        };

        // Setup mocks
        _mockItemRepository = new Mock<IItemRepository>();
        _mockItemFactory = new Mock<IInvoiceItemFactory>();

        // Repository returns test item for valid ID
        _mockItemRepository.Setup(r => r.GetItemById(_validItemId)).Returns(_testItem);

        // Factory setup
        _mockItemFactory.Setup(f => f.CanCreate(_testItem)).Returns(true);
        _mockItemFactory.Setup(f => f.Create(_testItem, _testDate)).Returns(_testInvoiceItem);

        // Create manager with mocks
        SUT = new InvoiceItemManager(_mockItemRepository.Object, [_mockItemFactory.Object]);
    }

    [Fact]
    public void Create_WithValidItemAndFactory_ReturnsInvoiceItem()
    {
        // Act
        var result = SUT.Create(_validItemId, _testDate, _testQuantity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_validItemId, result.ItemId);
        Assert.Equal(_testQuantity, result.Quantity);

        // Verify factory was called
        _mockItemFactory.Verify(f => f.Create(_testItem, _testDate), Times.Once);
    }

    [Fact]
    public void Create_WithInvalidItemId_ThrowsArgumentException()
    {
        // Arrange
        var invalidItemId = Guid.NewGuid();
        _mockItemRepository.Setup(r => r.GetItemById(invalidItemId)).Returns((Item)null!);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            SUT.Create(invalidItemId, _testDate, _testQuantity));

        Assert.Equal("Invalid item ID.", exception.Message);
    }

    [Fact]
    public void Create_WithNoMatchingFactory_ThrowsInvalidOperationException()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new MockItem(itemId, "TEST002", "Test Item 2", "Description",
            ItemType.Product, ItemCategory.GeneralGoods, new TaxCode());

        _mockItemRepository.Setup(r => r.GetItemById(itemId)).Returns(item);
        _mockItemFactory.Setup(f => f.CanCreate(item)).Returns(false);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            SUT.Create(itemId, _testDate, _testQuantity));

        Assert.Equal("No factory available to create the invoice item.", exception.Message);
    }

    [Fact]
    public void Create_WithValidInputs_SetsQuantityCorrectly()
    {
        // Arrange
        var expectedQuantity = 7.5m;

        // Act
        var result = SUT.Create(_validItemId, _testDate, expectedQuantity);

        // Assert
        Assert.Equal(expectedQuantity, result.Quantity);
    }

    [Fact]
    public void Create_WithMultipleFactories_SelectsCorrectFactory()
    {
        // Arrange
        var mockFactory1 = new Mock<IInvoiceItemFactory>();
        var mockFactory2 = new Mock<IInvoiceItemFactory>();

        mockFactory1.Setup(f => f.CanCreate(_testItem)).Returns(false);
        mockFactory2.Setup(f => f.CanCreate(_testItem)).Returns(true);
        mockFactory2.Setup(f => f.Create(_testItem, _testDate)).Returns(_testInvoiceItem);

        var manager = new InvoiceItemManager(
            _mockItemRepository.Object,
            [mockFactory1.Object, mockFactory2.Object]
        );

        // Act
        var result = manager.Create(_validItemId, _testDate, _testQuantity);

        // Assert
        Assert.NotNull(result);
        mockFactory1.Verify(f => f.Create(It.IsAny<Item>(), It.IsAny<DateOnly>()), Times.Never);
        mockFactory2.Verify(f => f.Create(_testItem, _testDate), Times.Once);
    }

    // Helper class for testing
    private sealed class MockItem : Item
    {
        public MockItem(Guid id, string sku, string name, string description,
            ItemType itemType, ItemCategory itemCategory, TaxCode taxCode)
            : base(id, sku, name, description, itemType, itemCategory, taxCode)
        {
        }

        public override ItemType ItemType => ItemType.Fee;
    }
}

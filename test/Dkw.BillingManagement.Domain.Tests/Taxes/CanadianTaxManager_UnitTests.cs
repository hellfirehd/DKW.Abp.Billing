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
using Dkw.BillingManagement.Invoices;
using Dkw.BillingManagement.Invoices.LineItems;
using Dkw.BillingManagement.Items;
using Dkw.BillingManagement.Provinces;

namespace Dkw.BillingManagement.Taxes;

public class CanadianTaxManager_UnitTests
{
    private static readonly DateOnly Epoch = new(1970, 01, 01);
    private static readonly DateOnly TestDate = new(2025, 01, 01);

    private readonly Mock<IProvinceRepository> _mockProvinceRepository;
    private readonly Mock<ITaxCodeRepository> _mockTaxCodeRepository;
    private readonly CustomerTaxProfile _notExemptCustomerProfile;
    private readonly CustomerTaxProfile _exemptCustomerProfile;

    private readonly Guid _testItemId = Guid.NewGuid();
    private readonly ItemBase _testItem;

    private readonly Guid _testLineItemId = Guid.NewGuid();
    private readonly LineItem _testLineItem;

    private readonly DateOnly _testDate = new(2023, 1, 1);
    private readonly List<ApplicableTax> _standardTaxes;
    private readonly Tax _gst;
    private readonly Tax _pst;
    private readonly Customer _customer = default!;
    private readonly Province _placeOfSupply = default!;

    private CanadianTaxManager SUT { get; }

    public CanadianTaxManager_UnitTests()
    {
        // Setup test data
        _testItem = Product.Create(_testItemId, "TEST001", "Test Item", "ZR-GROCERY", itemCategory: ItemCategory.BasicGroceries);
        _testItem.AddPrice(new ItemPrice(50.00m, Epoch));

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

        _gst = new Tax(Guid.NewGuid(), "GST", "GST").AddTaxRate(0.05m, new DateOnly(2020, 1, 1));
        _pst = new Tax(Guid.NewGuid(), "PST", "PST").AddTaxRate(0.07m, new DateOnly(2020, 1, 1));

        _standardTaxes = [_gst.GetTaxRate(TestDate), _pst.GetTaxRate(TestDate)];

        _placeOfSupply = Mock.Of<Province>(); //new (Guid.NewGuid(), "Timbuctoo", "TT", hasHst: false);
        Mock.Get(_placeOfSupply)
            .Setup(p => p.Taxes)
            .Returns([_gst, _pst]);

        _customer = TestData.Customer(_placeOfSupply);

        _notExemptCustomerProfile = Mock.Of<CustomerTaxProfile>(p => p.QualifiesForExemption(It.IsAny<DateOnly>()) == false);
        Mock.Get(_notExemptCustomerProfile)
            .Setup(p => p.PlaceOfSupply)
            .Returns(_placeOfSupply);

        _exemptCustomerProfile = Mock.Of<CustomerTaxProfile>(p => p.QualifiesForExemption(It.IsAny<DateOnly>()) == true);
        Mock.Get(_exemptCustomerProfile)
            .Setup(p => p.PlaceOfSupply)
            .Returns(_placeOfSupply);

        // Setup mocks
        _mockProvinceRepository = new Mock<IProvinceRepository>();
        _mockProvinceRepository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<Boolean>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_placeOfSupply);

        _mockTaxCodeRepository = new Mock<ITaxCodeRepository>();

        ////Default setup for tax provider

        //_mockTaxProvider.Setup(p => p.GetTaxesAsync(It.IsAny<Province>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(_standardTaxes);

        SUT = new CanadianTaxManager(_mockProvinceRepository.Object, _mockTaxCodeRepository.Object, TimeProvider.System);
    }

    [Fact]
    public async Task GetTaxRatesForItem_WhenItemClassificationIsInvalid_ReturnsStandardRates()
    {
        // Arrange
        var invalidClassification = new TaxClassification
        {
            ItemId = _testLineItem.ItemInfo.ItemId,
            TaxCode = "INVALID",
            AssignedDate = new DateOnly(2020, 1, 1),
            ExpirationDate = new DateOnly(2022, 12, 31) // Expired before effective date
        };

        _testLineItem.TaxClasification = invalidClassification;

        // Act
        var result = await SUT.GetTaxesAsync(_testLineItem, _notExemptCustomerProfile, TestDate);

        // Assert
        _mockTaxCodeRepository.Verify(r => r.GetAsync(It.IsAny<String>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(_standardTaxes, result);
    }

    [Fact]
    public async Task GetTaxRatesForItem_WhenItemClassificationIsNull_ReturnsStandardRates()
    {
        // Arrange
        _testLineItem.TaxClasification = null!;

        // Act
        var result = await SUT.GetTaxesAsync(_testLineItem, _notExemptCustomerProfile, TestDate);

        // Assert
        Assert.Equal(_standardTaxes, result);
    }

    [Fact]
    public async Task GetTaxRatesForItem_WhenTaxCodeNotFound_ReturnsStandardRates()
    {
        // Arrange
        var validClassification = new TaxClassification
        {
            ItemId = _testLineItem.ItemInfo.ItemId,
            TaxCode = "TAXCODE001",
            AssignedDate = new DateOnly(2020, 1, 1),
            ExpirationDate = null
        };

        _testLineItem.TaxClasification = validClassification;

        // Setup tax code repository to return null
        _mockTaxCodeRepository.Setup(r => r.GetAsync("TAXCODE001", It.IsAny<CancellationToken>())).ReturnsAsync((TaxCode)null!);

        // Act
        var result = await SUT.GetTaxesAsync(_testLineItem, _notExemptCustomerProfile, TestDate);

        // Assert
        Assert.Equal(_standardTaxes, result);
    }

    [Fact]
    public async Task GetTaxRatesForItem_WhenTaxCodeIsInvalid_ReturnsStandardRates()
    {
        // Arrange
        var validClassification = new TaxClassification
        {
            ItemId = _testLineItem.ItemInfo.ItemId,
            TaxCode = "TAXCODE001",
            AssignedDate = new DateOnly(2020, 1, 1)
        };

        _testLineItem.TaxClasification = validClassification;

        // Setup tax code repository to return an invalid tax code
        var invalidTaxCode = new TaxCode
        {
            Code = "TAXCODE001",
            Description = "Invalid Tax Code",
            EffectiveDate = new DateOnly(2020, 1, 1),
            ExpirationDate = new DateOnly(2022, 12, 31) // Expired before effective date
        };

        _mockTaxCodeRepository.Setup(r => r.GetAsync("TAXCODE001", It.IsAny<CancellationToken>())).ReturnsAsync(invalidTaxCode);

        // Act
        var result = await SUT.GetTaxesAsync(_testLineItem, _notExemptCustomerProfile, TestDate);

        // Assert
        Assert.Equal(_standardTaxes, result);
    }

    [Theory]
    [InlineData(TaxTreatment.Exempt)]
    [InlineData(TaxTreatment.OutOfScope)]
    public async Task GetTaxRatesForItem_WhenTaxTreatmentIsExemptOrOutOfScope_ReturnsEmptyCollection(TaxTreatment treatment)
    {
        // Arrange
        var validClassification = new TaxClassification
        {
            ItemId = _testLineItem.ItemInfo.ItemId,
            TaxCode = "EXEMPT001",
            AssignedDate = new DateOnly(2020, 1, 1)
        };

        _testLineItem.TaxClasification = validClassification;

        // Setup tax code repository to return a valid exempt tax code
        var taxCode = new TaxCode
        {
            Code = "EXEMPT001",
            Description = "Exempt Tax Code",
            TaxTreatment = treatment,
            EffectiveDate = new DateOnly(2020, 1, 1)
        };

        _mockTaxCodeRepository.Setup(r => r.GetAsync("EXEMPT001", It.IsAny<CancellationToken>())).ReturnsAsync(taxCode);

        // Act
        var result = await SUT.GetTaxesAsync(_testLineItem, _exemptCustomerProfile, TestDate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTaxRatesForItem_WhenTaxTreatmentIsZeroRated_ReturnsZeroRatedTaxes()
    {
        // Arrange
        var validClassification = new TaxClassification
        {
            ItemId = _testLineItem.ItemInfo.ItemId,
            TaxCode = "ZERO001",
            AssignedDate = new DateOnly(2020, 1, 1)
        };

        _testLineItem.TaxClasification = validClassification;

        // Setup tax code repository to return a valid zero-rated tax code
        var taxCode = new TaxCode
        {
            Code = "ZERO001",
            Description = "Zero-Rated Tax Code",
            TaxTreatment = TaxTreatment.ZeroRated,
            EffectiveDate = new DateOnly(2020, 1, 1)
        };

        _mockTaxCodeRepository.Setup(r => r.GetAsync("ZERO001", It.IsAny<CancellationToken>())).ReturnsAsync(taxCode);

        // Act
        var result = await SUT.GetTaxesAsync(_testLineItem, _notExemptCustomerProfile, TestDate);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(_standardTaxes.Count, list.Count);
        Assert.All(list, rate => Assert.Equal(0m, rate.Rate));
        Assert.Equal("GST", list[0].Code);
        Assert.Equal("PST", list[1].Code);
    }

    [Fact]
    public async Task GetTaxRatesForItem_WhenTaxTreatmentIsStandard_ReturnsStandardRates()
    {
        // Arrange
        var validClassification = new TaxClassification
        {
            ItemId = _testLineItem.ItemInfo.ItemId,
            TaxCode = "STANDARD001",
            AssignedDate = new DateOnly(2020, 1, 1)
        };

        _testLineItem.TaxClasification = validClassification;

        // Setup tax code repository to return a valid standard tax code
        var taxCode = new TaxCode
        {
            Code = "STANDARD001",
            Description = "Standard Tax Code",
            TaxTreatment = TaxTreatment.Standard,
            EffectiveDate = new DateOnly(2020, 1, 1)
        };

        _mockTaxCodeRepository.Setup(r => r.GetAsync("STANDARD001", It.IsAny<CancellationToken>())).ReturnsAsync(taxCode);

        // Act
        var result = await SUT.GetTaxesAsync(_testLineItem, _notExemptCustomerProfile, TestDate);

        // Assert
        Assert.Equal(_standardTaxes, result);
    }
}

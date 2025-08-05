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
using Dkw.BillingManagement.Items;
using Dkw.BillingManagement.Provinces;
using Dkw.BillingManagement.Taxes;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Modularity;

namespace Dkw.BillingManagement;

public class InvoiceManager_Tests<TStartupModule> : BillingManagemenDomainTestBase<TStartupModule, InvoiceManager>
    where TStartupModule : IAbpModule
{
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly Mock<ITaxCodeRepository> _mockTaxCodeRepository;
    private readonly Mock<ITaxProvider> _mockTaxProvider;
    private readonly Mock<IProvinceRepository> _mockProvinceManager;

    private readonly CustomerTaxProfile _notExemptCustomerProfile;
    private readonly CustomerTaxProfile _exemptCustomerProfile;

    private readonly Guid _testItemId = Guid.NewGuid();
    private readonly ItemBase _testItem;

    private readonly Guid _testLineItemId = Guid.NewGuid();
    private readonly LineItem _testLineItem;

    private readonly DateOnly _testDate = new(2023, 1, 1);
    private readonly List<ApplicableTax> _applicableTaxes;

    public InvoiceManager_Tests()
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

        _notExemptCustomerProfile = Mock.Of<CustomerTaxProfile>(p => p.QualifiesForExemption(It.IsAny<DateOnly>()) == false);
        _exemptCustomerProfile = Mock.Of<CustomerTaxProfile>(p => p.QualifiesForExemption(It.IsAny<DateOnly>()) == true);

        _applicableTaxes =
        [
            new(Guid.NewGuid(), "Goods and Services Tax", "GST", 0.05m),
            new(Guid.NewGuid(), "Provincial Sales Tax", "PST", 0.07m)
        ];

        // Setup mocks
        _mockItemRepository = new Mock<IItemRepository>();
        _mockTaxCodeRepository = new Mock<ITaxCodeRepository>();
        _mockTaxProvider = new Mock<ITaxProvider>();
        _mockProvinceManager = new Mock<IProvinceRepository>(new Mock<IProvinceRepository>().Object);

        // Default setup for tax provider
        _mockTaxProvider.Setup(p => p.GetApplicableTaxesAsync(It.IsAny<Province>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_applicableTaxes);
    }

    protected override void BeforeAddApplication(IServiceCollection services)
    {
        services.AddTransient(_ => _mockItemRepository.Object);
        services.AddTransient(_ => _mockTaxCodeRepository.Object);
        services.AddTransient(_ => _mockTaxProvider.Object);
        services.AddTransient(_ => _mockProvinceManager.Object);
    }

    [Fact]
    public async Task GetTaxRatesForItem_WhenCustomerQualifiesForExemption_ReturnsEmptyCollection()
    {
        // Act
        var result = await SUT.GetApplicableTaxesAsync(_testLineItem, _exemptCustomerProfile, _testDate);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
        _mockTaxProvider.Verify(p => p.GetApplicableTaxesAsync(It.IsAny<Province>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTaxRatesForItem_WhenItemClassificationIsNull_ReturnsStandardRates()
    {
        // Arrange
        _testLineItem.TaxClasification = null!;

        // Act
        var result = await SUT.GetApplicableTaxesAsync(_testLineItem, _notExemptCustomerProfile, _testDate);

        // Assert
        Assert.Equal(_applicableTaxes, result);
        _mockTaxProvider.Verify(p => p.GetApplicableTaxesAsync(It.IsAny<Province>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
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
        var result = await SUT.GetApplicableTaxesAsync(_testLineItem, _notExemptCustomerProfile, _testDate);

        // Assert
        Assert.Equal(_applicableTaxes, result);
        _mockTaxProvider.Verify(p => p.GetApplicableTaxesAsync(It.IsAny<Province>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
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
        var result = await SUT.GetApplicableTaxesAsync(_testLineItem, _notExemptCustomerProfile, _testDate);

        // Assert
        Assert.Equal(_applicableTaxes, result);
        _mockTaxProvider.Verify(p => p.GetApplicableTaxesAsync(It.IsAny<Province>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
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
        var result = await SUT.GetApplicableTaxesAsync(_testLineItem, _notExemptCustomerProfile, _testDate);

        // Assert
        Assert.Equal(_applicableTaxes, result);
        _mockTaxProvider.Verify(p => p.GetApplicableTaxesAsync(It.IsAny<Province>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
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
        var result = await SUT.GetApplicableTaxesAsync(_testLineItem, _exemptCustomerProfile, _testDate);

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
        var result = await SUT.GetApplicableTaxesAsync(_testLineItem, _notExemptCustomerProfile, _testDate);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(_applicableTaxes.Count, list.Count);
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
        var result = await SUT.GetApplicableTaxesAsync(_testLineItem, _notExemptCustomerProfile, _testDate);

        // Assert
        Assert.Equal(_applicableTaxes, result);
        _mockTaxProvider.Verify(p => p.GetApplicableTaxesAsync(It.IsAny<Province>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

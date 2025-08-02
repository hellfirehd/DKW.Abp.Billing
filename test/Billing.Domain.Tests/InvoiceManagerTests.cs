using Billing;

namespace Dkw.Abp.Billing;

public class InvoiceManagerTests
{
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly Mock<ITaxCodeRepository> _mockTaxCodeRepository;
    private readonly Mock<ITaxProvider> _mockTaxProvider;
    private readonly Mock<ProvinceManager> _mockProvinceManager;
    private readonly Mock<InvoiceItemManager> _mockInvoiceItemManager;

    private readonly CustomerTaxProfile _notExemptCustomerProfile;
    private readonly CustomerTaxProfile _exemptCustomerProfile;

    private readonly InvoiceItem _testInvoiceItem;
    private readonly DateOnly _effectiveDate = new(2023, 1, 1);
    private readonly Province _testProvince;
    private readonly List<TaxRate> _standardRates;
    private InvoiceManager SUT { get; }

    public InvoiceManagerTests()
    {
        // Setup test data
        _testInvoiceItem = new InvoiceItem
        {
            Id = Guid.NewGuid(),
            ItemId = Guid.NewGuid(),
            SKU = "TEST001",
            Name = "Test Item",
            UnitPrice = 100.0m,
            InvoiceDate = _effectiveDate
        };

        _notExemptCustomerProfile = Mock.Of<CustomerTaxProfile>(p => p.QualifiesForExemption(It.IsAny<DateOnly>()) == false);
        _exemptCustomerProfile = Mock.Of<CustomerTaxProfile>(p => p.QualifiesForExemption(It.IsAny<DateOnly>()) == true);

        _standardRates =
        [
            new(new Tax("GST", "Goods and Services Tax", TaxJurisdiction.Federal), 0.05m ,new DateOnly(2008, 1, 1)),
            new(new Tax("PST", "Provincial Sales Tax", TaxJurisdiction.Provincial), 0.07m, new DateOnly(2013, 4, 1))
        ];

        // Setup mocks
        _mockItemRepository = new Mock<IItemRepository>();
        _mockTaxCodeRepository = new Mock<ITaxCodeRepository>();
        _mockTaxProvider = new Mock<ITaxProvider>();
        _mockProvinceManager = new Mock<ProvinceManager>(new Mock<IProvinceRepository>().Object);
        _mockInvoiceItemManager = new Mock<InvoiceItemManager>(
            new Mock<IItemRepository>().Object,
            new List<IInvoiceItemFactory>());

        // Default setup for tax provider
        _mockTaxProvider.Setup(p => p.GetTaxRates(It.IsAny<Province>(), It.IsAny<DateOnly>()))
            .Returns(_standardRates);

        _testProvince = TestData.Alberta;

        // Create the invoice manager
        SUT = new InvoiceManager(
            _mockItemRepository.Object,
            _mockTaxCodeRepository.Object,
            _mockTaxProvider.Object,
            _mockProvinceManager.Object,
            _mockInvoiceItemManager.Object
        );
    }

    [Fact]
    public void ClassifyItem_ShouldThrowNotImplementedException()
    {
        // Act & Assert
        var exception = Assert.Throws<NotImplementedException>(() =>
            SUT.ClassifyItem(_testInvoiceItem));

        Assert.Equal("Item classification logic is not implemented yet.", exception.Message);
    }

    [Fact]
    public void GetTaxRatesForItem_WhenCustomerQualifiesForExemption_ReturnsEmptyCollection()
    {
        // Act
        var result = SUT.GetTaxRatesForItem(_testInvoiceItem, _exemptCustomerProfile, _effectiveDate);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
        _mockTaxProvider.Verify(p => p.GetTaxRates(It.IsAny<Province>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public void GetTaxRatesForItem_WhenItemClassificationIsNull_ReturnsStandardRates()
    {
        // Arrange
        _testInvoiceItem.TaxClasification = null!;

        // Act
        var result = SUT.GetTaxRatesForItem(_testInvoiceItem, _notExemptCustomerProfile, _effectiveDate);

        // Assert
        Assert.Equal(_standardRates, result);
        _mockTaxProvider.Verify(p => p.GetTaxRates(It.IsAny<Province>(), It.IsAny<DateOnly>()), Times.Once);
    }

    [Fact]
    public void GetTaxRatesForItem_WhenItemClassificationIsInvalid_ReturnsStandardRates()
    {
        // Arrange
        var invalidClassification = new TaxClassification
        {
            Id = Guid.NewGuid(),
            ItemId = _testInvoiceItem.ItemId,
            TaxCode = "INVALID",
            AssignedDate = new DateOnly(2020, 1, 1),
            ExpirationDate = new DateOnly(2022, 12, 31) // Expired before effective date
        };

        _testInvoiceItem.TaxClasification = invalidClassification;

        // Act
        var result = SUT.GetTaxRatesForItem(_testInvoiceItem, _notExemptCustomerProfile, _effectiveDate);

        // Assert
        Assert.Equal(_standardRates, result);
        _mockTaxProvider.Verify(p => p.GetTaxRates(It.IsAny<Province>(), It.IsAny<DateOnly>()), Times.Once);
    }

    [Fact]
    public void GetTaxRatesForItem_WhenTaxCodeNotFound_ReturnsStandardRates()
    {
        // Arrange
        var validClassification = new TaxClassification
        {
            Id = Guid.NewGuid(),
            ItemId = _testInvoiceItem.ItemId,
            TaxCode = "TAXCODE001",
            AssignedDate = new DateOnly(2020, 1, 1),
            ExpirationDate = null
        };

        _testInvoiceItem.TaxClasification = validClassification;

        // Setup tax code repository to return null
        _mockTaxCodeRepository.Setup(r => r.GetByCode("TAXCODE001")).Returns((TaxCode)null!);

        // Act
        var result = SUT.GetTaxRatesForItem(_testInvoiceItem, _notExemptCustomerProfile, _effectiveDate);

        // Assert
        Assert.Equal(_standardRates, result);
        _mockTaxProvider.Verify(p => p.GetTaxRates(It.IsAny<Province>(), It.IsAny<DateOnly>()), Times.Once);
    }

    [Fact]
    public void GetTaxRatesForItem_WhenTaxCodeIsInvalid_ReturnsStandardRates()
    {
        // Arrange
        var validClassification = new TaxClassification
        {
            Id = Guid.NewGuid(),
            ItemId = _testInvoiceItem.ItemId,
            TaxCode = "TAXCODE001",
            AssignedDate = new DateOnly(2020, 1, 1)
        };

        _testInvoiceItem.TaxClasification = validClassification;

        // Setup tax code repository to return an invalid tax code
        var invalidTaxCode = new TaxCode
        {
            Code = "TAXCODE001",
            Description = "Invalid Tax Code",
            EffectiveDate = new DateOnly(2020, 1, 1),
            ExpirationDate = new DateOnly(2022, 12, 31) // Expired before effective date
        };

        _mockTaxCodeRepository.Setup(r => r.GetByCode("TAXCODE001")).Returns(invalidTaxCode);

        // Act
        var result = SUT.GetTaxRatesForItem(_testInvoiceItem, _notExemptCustomerProfile, _effectiveDate);

        // Assert
        Assert.Equal(_standardRates, result);
        _mockTaxProvider.Verify(p => p.GetTaxRates(It.IsAny<Province>(), It.IsAny<DateOnly>()), Times.Once);
    }

    [Theory]
    [InlineData(TaxTreatment.Exempt)]
    [InlineData(TaxTreatment.OutOfScope)]
    public void GetTaxRatesForItem_WhenTaxTreatmentIsExemptOrOutOfScope_ReturnsEmptyCollection(TaxTreatment treatment)
    {
        // Arrange
        var validClassification = new TaxClassification
        {
            Id = Guid.NewGuid(),
            ItemId = _testInvoiceItem.ItemId,
            TaxCode = "EXEMPT001",
            AssignedDate = new DateOnly(2020, 1, 1)
        };

        _testInvoiceItem.TaxClasification = validClassification;

        // Setup tax code repository to return a valid exempt tax code
        var taxCode = new TaxCode
        {
            Code = "EXEMPT001",
            Description = "Exempt Tax Code",
            TaxTreatment = treatment,
            EffectiveDate = new DateOnly(2020, 1, 1)
        };

        _mockTaxCodeRepository.Setup(r => r.GetByCode("EXEMPT001")).Returns(taxCode);

        // Act
        var result = SUT.GetTaxRatesForItem(_testInvoiceItem, _exemptCustomerProfile, _effectiveDate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetTaxRatesForItem_WhenTaxTreatmentIsZeroRated_ReturnsZeroRatedTaxes()
    {
        // Arrange
        var validClassification = new TaxClassification
        {
            Id = Guid.NewGuid(),
            ItemId = _testInvoiceItem.ItemId,
            TaxCode = "ZERO001",
            AssignedDate = new DateOnly(2020, 1, 1)
        };

        _testInvoiceItem.TaxClasification = validClassification;

        // Setup tax code repository to return a valid zero-rated tax code
        var taxCode = new TaxCode
        {
            Code = "ZERO001",
            Description = "Zero-Rated Tax Code",
            TaxTreatment = TaxTreatment.ZeroRated,
            EffectiveDate = new DateOnly(2020, 1, 1)
        };

        _mockTaxCodeRepository.Setup(r => r.GetByCode("ZERO001")).Returns(taxCode);

        // Act
        var result = SUT.GetTaxRatesForItem(_testInvoiceItem, _notExemptCustomerProfile, _effectiveDate).ToList();

        // Assert
        Assert.Equal(_standardRates.Count, result.Count);
        Assert.All(result, rate => Assert.Equal(0m, rate.Rate));
        Assert.Equal("GST", result[0].Code);
        Assert.Equal("PST", result[1].Code);
    }

    [Fact]
    public void GetTaxRatesForItem_WhenTaxTreatmentIsStandard_ReturnsStandardRates()
    {
        // Arrange
        var validClassification = new TaxClassification
        {
            Id = Guid.NewGuid(),
            ItemId = _testInvoiceItem.ItemId,
            TaxCode = "STANDARD001",
            AssignedDate = new DateOnly(2020, 1, 1)
        };

        _testInvoiceItem.TaxClasification = validClassification;

        // Setup tax code repository to return a valid standard tax code
        var taxCode = new TaxCode
        {
            Code = "STANDARD001",
            Description = "Standard Tax Code",
            TaxTreatment = TaxTreatment.Standard,
            EffectiveDate = new DateOnly(2020, 1, 1)
        };

        _mockTaxCodeRepository.Setup(r => r.GetByCode("STANDARD001")).Returns(taxCode);

        // Act
        var result = SUT.GetTaxRatesForItem(_testInvoiceItem, _notExemptCustomerProfile, _effectiveDate);

        // Assert
        Assert.Equal(_standardRates, result);
        _mockTaxProvider.Verify(p => p.GetTaxRates(It.IsAny<Province>(), It.IsAny<DateOnly>()), Times.Once);
    }
}

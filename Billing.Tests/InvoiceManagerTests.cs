using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;

namespace Billing.Tests
{
    public class InvoiceManagerTests
    {
        private readonly Mock<IItemRepository> _mockItemRepository;
        private readonly Mock<ITaxCodeRepository> _mockTaxCodeRepository;
        private readonly Mock<ITaxProvider> _mockTaxProvider;
        private readonly Mock<ProvinceManager> _mockProvinceManager;
        private readonly Mock<InvoiceItemManager> _mockInvoiceItemManager;
        private readonly InvoiceManager _invoiceManager;
        
        private readonly InvoiceItem _testInvoiceItem;
        private readonly CustomerTaxProfile _testCustomerProfile;
        private readonly DateOnly _effectiveDate = new DateOnly(2023, 1, 1);
        private readonly Province _testProvince = Province.Create("AB", "Alberta");
        private readonly List<TaxRate> _standardRates;

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

            _testCustomerProfile = new CustomerTaxProfile
            {
                Id = Guid.NewGuid(),
                CustomerId = "CUST001",
                TaxProvince = _testProvince,
                IsEligibleForExemption = false,
                EffectiveDate = new DateOnly(2020, 1, 1),
                IsActive = true
            };

            _standardRates = new List<TaxRate>
            {
                new TaxRate { Code = "GST", Name = "Goods and Services Tax", Rate = 0.05m },
                new TaxRate { Code = "PST", Name = "Provincial Sales Tax", Rate = 0.07m }
            };

            // Setup mocks
            _mockItemRepository = new Mock<IItemRepository>();
            _mockTaxCodeRepository = new Mock<ITaxCodeRepository>();
            _mockTaxProvider = new Mock<ITaxProvider>();
            _mockProvinceManager = new Mock<ProvinceManager>(new Mock<IProvinceRepository>().Object);
            _mockInvoiceItemManager = new Mock<InvoiceItemManager>(
                new Mock<IItemRepository>().Object, 
                new List<IInvoiceItemFactory>());

            // Default setup for tax provider
            _mockTaxProvider.Setup(p => p.GetTaxRates(_testProvince, _effectiveDate))
                .Returns(_standardRates);

            // Create the invoice manager
            _invoiceManager = new InvoiceManager(
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
                _invoiceManager.ClassifyItem(_testInvoiceItem));
                
            Assert.Equal("Item classification logic is not implemented yet.", exception.Message);
        }

        [Fact]
        public void GetTaxRatesForItem_WhenCustomerQualifiesForExemption_ReturnsEmptyCollection()
        {
            // Arrange
            var exemptCustomer = new CustomerTaxProfile
            {
                Id = Guid.NewGuid(),
                CustomerId = "EXEMPT001",
                TaxProvince = _testProvince,
                IsEligibleForExemption = true,
                EffectiveDate = new DateOnly(2020, 1, 1),
                IsActive = true
            };
            
            // Setup exemption check to return true
            Mock.Get(exemptCustomer)
                .Setup(c => c.QualifiesForExemption(_effectiveDate))
                .Returns(true);

            // Act
            var result = _invoiceManager.GetTaxRatesForItem(_testInvoiceItem, exemptCustomer, _effectiveDate);

            // Assert
            Assert.Empty(result);
            _mockTaxProvider.Verify(p => p.GetTaxRates(It.IsAny<Province>(), It.IsAny<DateOnly>()), Times.Never);
        }

        [Fact]
        public void GetTaxRatesForItem_WhenItemClassificationIsNull_ReturnsStandardRates()
        {
            // Arrange
            _testInvoiceItem.ItemClassification = null;
            
            // Setup mock customer
            var customerProfile = _testCustomerProfile;
            Mock.Get(customerProfile)
                .Setup(c => c.QualifiesForExemption(_effectiveDate))
                .Returns(false);

            // Act
            var result = _invoiceManager.GetTaxRatesForItem(_testInvoiceItem, customerProfile, _effectiveDate);

            // Assert
            Assert.Equal(_standardRates, result);
            _mockTaxProvider.Verify(p => p.GetTaxRates(_testProvince, _effectiveDate), Times.Once);
        }

        [Fact]
        public void GetTaxRatesForItem_WhenItemClassificationIsInvalid_ReturnsStandardRates()
        {
            // Arrange
            var invalidClassification = new ItemClassification
            {
                Id = Guid.NewGuid(),
                ItemId = _testInvoiceItem.ItemId,
                TaxCode = "INVALID",
                AssignedDate = new DateOnly(2020, 1, 1),
                ExpirationDate = new DateOnly(2022, 12, 31) // Expired before effective date
            };
            
            _testInvoiceItem.ItemClassification = invalidClassification;
            
            // Setup mock customer
            var customerProfile = _testCustomerProfile;
            Mock.Get(customerProfile)
                .Setup(c => c.QualifiesForExemption(_effectiveDate))
                .Returns(false);

            // Act
            var result = _invoiceManager.GetTaxRatesForItem(_testInvoiceItem, customerProfile, _effectiveDate);

            // Assert
            Assert.Equal(_standardRates, result);
            _mockTaxProvider.Verify(p => p.GetTaxRates(_testProvince, _effectiveDate), Times.Once);
        }

        [Fact]
        public void GetTaxRatesForItem_WhenTaxCodeNotFound_ReturnsStandardRates()
        {
            // Arrange
            var validClassification = new ItemClassification
            {
                Id = Guid.NewGuid(),
                ItemId = _testInvoiceItem.ItemId,
                TaxCode = "TAXCODE001",
                AssignedDate = new DateOnly(2020, 1, 1),
                ExpirationDate = null
            };
            
            _testInvoiceItem.ItemClassification = validClassification;
            
            // Setup tax code repository to return null
            _mockTaxCodeRepository.Setup(r => r.GetByCode("TAXCODE001")).Returns((TaxCode)null);
            
            // Setup mock customer
            var customerProfile = _testCustomerProfile;
            Mock.Get(customerProfile)
                .Setup(c => c.QualifiesForExemption(_effectiveDate))
                .Returns(false);

            // Act
            var result = _invoiceManager.GetTaxRatesForItem(_testInvoiceItem, customerProfile, _effectiveDate);

            // Assert
            Assert.Equal(_standardRates, result);
            _mockTaxProvider.Verify(p => p.GetTaxRates(_testProvince, _effectiveDate), Times.Once);
        }

        [Fact]
        public void GetTaxRatesForItem_WhenTaxCodeIsInvalid_ReturnsStandardRates()
        {
            // Arrange
            var validClassification = new ItemClassification
            {
                Id = Guid.NewGuid(),
                ItemId = _testInvoiceItem.ItemId,
                TaxCode = "TAXCODE001",
                AssignedDate = new DateOnly(2020, 1, 1)
            };
            
            _testInvoiceItem.ItemClassification = validClassification;
            
            // Setup tax code repository to return an invalid tax code
            var invalidTaxCode = new TaxCode
            {
                Code = "TAXCODE001",
                Description = "Invalid Tax Code",
                EffectiveDate = new DateOnly(2020, 1, 1),
                ExpirationDate = new DateOnly(2022, 12, 31) // Expired before effective date
            };
            
            _mockTaxCodeRepository.Setup(r => r.GetByCode("TAXCODE001")).Returns(invalidTaxCode);
            
            // Setup mock customer
            var customerProfile = _testCustomerProfile;
            Mock.Get(customerProfile)
                .Setup(c => c.QualifiesForExemption(_effectiveDate))
                .Returns(false);

            // Act
            var result = _invoiceManager.GetTaxRatesForItem(_testInvoiceItem, customerProfile, _effectiveDate);

            // Assert
            Assert.Equal(_standardRates, result);
            _mockTaxProvider.Verify(p => p.GetTaxRates(_testProvince, _effectiveDate), Times.Once);
        }

        [Theory]
        [InlineData(TaxTreatment.Exempt)]
        [InlineData(TaxTreatment.OutOfScope)]
        public void GetTaxRatesForItem_WhenTaxTreatmentIsExemptOrOutOfScope_ReturnsEmptyCollection(TaxTreatment treatment)
        {
            // Arrange
            var validClassification = new ItemClassification
            {
                Id = Guid.NewGuid(),
                ItemId = _testInvoiceItem.ItemId,
                TaxCode = "EXEMPT001",
                AssignedDate = new DateOnly(2020, 1, 1)
            };
            
            _testInvoiceItem.ItemClassification = validClassification;
            
            // Setup tax code repository to return a valid exempt tax code
            var taxCode = new TaxCode
            {
                Code = "EXEMPT001",
                Description = "Exempt Tax Code",
                TaxTreatment = treatment,
                EffectiveDate = new DateOnly(2020, 1, 1)
            };
            
            _mockTaxCodeRepository.Setup(r => r.GetByCode("EXEMPT001")).Returns(taxCode);
            
            // Setup mock customer
            var customerProfile = _testCustomerProfile;
            Mock.Get(customerProfile)
                .Setup(c => c.QualifiesForExemption(_effectiveDate))
                .Returns(false);

            // Act
            var result = _invoiceManager.GetTaxRatesForItem(_testInvoiceItem, customerProfile, _effectiveDate);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetTaxRatesForItem_WhenTaxTreatmentIsZeroRated_ReturnsZeroRatedTaxes()
        {
            // Arrange
            var validClassification = new ItemClassification
            {
                Id = Guid.NewGuid(),
                ItemId = _testInvoiceItem.ItemId,
                TaxCode = "ZERO001",
                AssignedDate = new DateOnly(2020, 1, 1)
            };
            
            _testInvoiceItem.ItemClassification = validClassification;
            
            // Setup tax code repository to return a valid zero-rated tax code
            var taxCode = new TaxCode
            {
                Code = "ZERO001",
                Description = "Zero-Rated Tax Code",
                TaxTreatment = TaxTreatment.ZeroRated,
                EffectiveDate = new DateOnly(2020, 1, 1)
            };
            
            _mockTaxCodeRepository.Setup(r => r.GetByCode("ZERO001")).Returns(taxCode);
            
            // Setup mock customer
            var customerProfile = _testCustomerProfile;
            Mock.Get(customerProfile)
                .Setup(c => c.QualifiesForExemption(_effectiveDate))
                .Returns(false);

            // Act
            var result = _invoiceManager.GetTaxRatesForItem(_testInvoiceItem, customerProfile, _effectiveDate).ToList();

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
            var validClassification = new ItemClassification
            {
                Id = Guid.NewGuid(),
                ItemId = _testInvoiceItem.ItemId,
                TaxCode = "STANDARD001",
                AssignedDate = new DateOnly(2020, 1, 1)
            };
            
            _testInvoiceItem.ItemClassification = validClassification;
            
            // Setup tax code repository to return a valid standard tax code
            var taxCode = new TaxCode
            {
                Code = "STANDARD001",
                Description = "Standard Tax Code",
                TaxTreatment = TaxTreatment.Standard,
                EffectiveDate = new DateOnly(2020, 1, 1)
            };
            
            _mockTaxCodeRepository.Setup(r => r.GetByCode("STANDARD001")).Returns(taxCode);
            
            // Setup mock customer
            var customerProfile = _testCustomerProfile;
            Mock.Get(customerProfile)
                .Setup(c => c.QualifiesForExemption(_effectiveDate))
                .Returns(false);

            // Act
            var result = _invoiceManager.GetTaxRatesForItem(_testInvoiceItem, customerProfile, _effectiveDate);

            // Assert
            Assert.Equal(_standardRates, result);
            _mockTaxProvider.Verify(p => p.GetTaxRates(_testProvince, _effectiveDate), Times.Once);
        }
    }
}
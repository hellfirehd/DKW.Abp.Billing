namespace Billing.Domain.Tests;

/// <summary>
/// Unit tests for the Canadian Tax Provider
/// </summary>
public class CanadianTaxProviderTests
{
    private readonly CanadianTaxProvider _taxProvider = new();
    private readonly ProvinceManager _pm = new();

    [Fact]
    public void GetTaxRates_ShouldReturnCorrectRates_ForNovaScotia()
    {
        // Arrange
        var effectiveDate = new DateOnly(2023, 6, 1);

        // Act
        var rates = _taxProvider.GetTaxRates(_pm.GetProvince("NS"), effectiveDate);

        // Assert
        Assert.Single(rates);
        Assert.Equal("NS-HST", rates[0].Code);
        Assert.Equal(0.15m, rates[0].TaxRate.Rate); // 15% HST
    }

    [Fact]
    public void GetTaxRates_ShouldReturnCorrectRates_ForBritishColumbia()
    {
        // Arrange
        var effectiveDate = new DateOnly(2023, 6, 1);

        // Act
        var rates = _taxProvider.GetTaxRates(_pm.GetProvince("BC"), effectiveDate);

        // Assert
        Assert.Equal(2, rates.Length); // GST + PST

        var gst = rates.First(r => r.Code == "GST");
        var pst = rates.First(r => r.Code == "BC-PST");

        Assert.Equal(0.05m, gst.TaxRate.Rate); // 5% GST
        Assert.Equal(0.07m, pst.TaxRate.Rate); // 7% PST
    }

    [Fact]
    public void GetTaxRatesForCategory_ShouldReturnEmptyArray_ForNonTaxableProducts()
    {
        // Arrange
        var effectiveDate = new DateOnly(2023, 6, 1);

        // Act
        var rates = _taxProvider.GetTaxRatesForCategory(_pm.GetProvince("BC"), effectiveDate, TaxCategory.NonTaxableProduct);

        // Assert
        Assert.Empty(rates);
    }

    [Fact]
    public void HasTaxConfiguration_ShouldReturnTrue_ForAllCanadianProvinces()
    {
        // Act & Assert
        Assert.True(_taxProvider.HasTaxConfiguration(_pm.GetProvince("AB")));
        Assert.True(_taxProvider.HasTaxConfiguration(_pm.GetProvince("BC")));
        Assert.True(_taxProvider.HasTaxConfiguration(_pm.GetProvince("MB")));
        Assert.True(_taxProvider.HasTaxConfiguration(_pm.GetProvince("NB")));
        Assert.True(_taxProvider.HasTaxConfiguration(_pm.GetProvince("NL")));
        Assert.True(_taxProvider.HasTaxConfiguration(_pm.GetProvince("NS")));
        Assert.True(_taxProvider.HasTaxConfiguration(_pm.GetProvince("ON")));
        Assert.True(_taxProvider.HasTaxConfiguration(_pm.GetProvince("PE")));
        Assert.True(_taxProvider.HasTaxConfiguration(_pm.GetProvince("QC")));
        Assert.True(_taxProvider.HasTaxConfiguration(_pm.GetProvince("SK")));
        Assert.True(_taxProvider.HasTaxConfiguration(_pm.GetProvince("NT")));
        Assert.True(_taxProvider.HasTaxConfiguration(_pm.GetProvince("NU")));
        Assert.True(_taxProvider.HasTaxConfiguration(_pm.GetProvince("YT")));
    }

    [Fact]
    public void GetSupportedProvinces_ShouldReturnAllCanadianProvinces()
    {
        // Act
        var provinces = _taxProvider.GetSupportedProvinces();

        // Assert
        Assert.Equal(13, provinces.Length); // All Canadian provinces and territories
        Assert.Contains(_pm.GetProvince("AB"), provinces);
        Assert.Contains(_pm.GetProvince("BC"), provinces);
        Assert.Contains(_pm.GetProvince("NS"), provinces);
        Assert.Contains(_pm.GetProvince("ON"), provinces);
        Assert.Contains(_pm.GetProvince("QC"), provinces);
    }

    [Theory]
    [InlineData("2023-06-01", 0.15)] // Current rate
    [InlineData("2025-05-01", 0.14)] // Future reduced rate
    public void GetTaxRates_ShouldReturnCorrectHistoricalRates_ForNovaScotia(string dateString, decimal expectedRate)
    {
        // Arrange
        var effectiveDate = DateOnly.Parse(dateString);

        // Act
        var rates = _taxProvider.GetTaxRates(_pm.GetProvince("NS"), effectiveDate);

        // Assert
        Assert.Single(rates);
        Assert.Equal(expectedRate, rates[0].TaxRate.Rate);
    }

    [Theory]
    [InlineData("AB", 1)] // Alberta - GST only
    [InlineData("BC", 2)] // BC - GST + PST  
    [InlineData("ON", 1)] // Ontario - HST only
    [InlineData("QC", 2)] // Quebec - GST + QST
    [InlineData("NT", 1)] // Northwest Territories - GST only
    [InlineData("NU", 1)] // Nunavut - GST only
    [InlineData("YT", 1)] // Yukon - GST only
    public void GetTaxRates_ShouldReturnCorrectNumberOfTaxes_ByProvince(string provinceCode, int expectedCount)
    {
        // Arrange
        var province = _pm.GetProvince(provinceCode);
        var effectiveDate = new DateOnly(2023, 6, 1);

        // Act
        var rates = _taxProvider.GetTaxRates(province, effectiveDate);

        // Assert
        Assert.Equal(expectedCount, rates.Length);
    }

    [Theory]
    [InlineData("AB")] // Alberta - GST only
    [InlineData("NT")] // Northwest Territories - GST only  
    [InlineData("NU")] // Nunavut - GST only
    [InlineData("YT")] // Yukon - GST only
    public void GetTaxRates_ShouldReturnGSTOnly_ForGSTOnlyProvinces(string provinceCode)
    {
        // Arrange
        var province = _pm.GetProvince(provinceCode);
        var effectiveDate = new DateOnly(2023, 6, 1);

        // Act
        var rates = _taxProvider.GetTaxRates(province, effectiveDate);

        // Assert
        Assert.Single(rates);
        Assert.Equal("GST", rates[0].Code);
        Assert.Equal(0.05m, rates[0].TaxRate.Rate);
    }

    [Fact]
    public void TaxCodeSystem_ConceptDemo_ShouldClassifyBasicGroceries_AsZeroRated()
    {
        // This test demonstrates the concept for future tax code implementation
        // Basic groceries should be zero-rated (0% but still tracked)

        // For now, we simulate this with existing structure
        var groceries = new Product
        {
            Description = "Basic Groceries (Milk, Bread, Eggs)",
            UnitPrice = 25.00m,
            TaxCategory = TaxCategory.NonTaxableProduct // Simulating zero-rated
        };

        var invoice = new Invoice { Province = _pm.GetProvince("BC") };
        invoice.AddItem(groceries);

        var taxRates = _taxProvider.GetTaxRatesForCategory(
            invoice.Province,
            invoice.InvoiceDate,
            groceries.TaxCategory);

        groceries.ApplyTaxes(taxRates);

        // Should have no tax (simulating zero-rated for now)
        Assert.Equal(0m, groceries.GetTotalTax());
        Assert.Equal(25.00m, groceries.GetTotal());

        // Future enhancement: This would track as zero-rated rather than non-taxable
        // to support proper GST/HST compliance reporting
    }

    [Fact]
    public void TaxCodeSystem_ConceptDemo_ShouldHandleCustomerExemptions()
    {
        // This test demonstrates customer-level exemptions concept

        var standardProduct = new Product
        {
            Description = "Office Supplies",
            UnitPrice = 100.00m,
            TaxCategory = TaxCategory.TaxableProduct
        };

        var invoice = new Invoice { Province = _pm.GetProvince("BC") };
        invoice.AddItem(standardProduct);

        // Test 1: Regular customer gets full tax
        var regularTaxRates = _taxProvider.GetTaxRatesForCategory(
            invoice.Province,
            invoice.InvoiceDate,
            standardProduct.TaxCategory);

        standardProduct.ApplyTaxes(regularTaxRates);
        var regularTax = standardProduct.GetTotalTax();

        // Reset for exempt customer test
        standardProduct.AppliedTaxes.Clear();

        // Test 2: Exempt customer gets no tax (simulated by applying no rates)
        var exemptTaxRates = Array.Empty<ItemTaxRate>(); // Simulating exemption
        standardProduct.ApplyTaxes(exemptTaxRates);
        var exemptTax = standardProduct.GetTotalTax();

        Assert.True(regularTax > 0);
        Assert.Equal(0m, exemptTax);

        // Future enhancement: This would be based on CustomerTaxProfile
        // with proper exemption certificate validation
    }
}

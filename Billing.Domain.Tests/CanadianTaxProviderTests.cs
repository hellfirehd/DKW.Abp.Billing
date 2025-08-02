namespace Billing.Domain.Tests;

/// <summary>
/// Unit tests for the Canadian Tax Provider
/// </summary>
public class CanadianTaxProviderTests
{
    private static readonly DateOnly EffectiveDate = new(2025, 01, 01);

    private CanadianTaxProvider SUT { get; } = new CanadianTaxProvider(TestData.ProvinceManager, TimeProvider.System);

    [Fact]
    public void GetTaxRates_ShouldReturnCorrectRates_ForNovaScotia()
    {
        // Arrange
        var effectiveDate = new DateOnly(2023, 6, 1);

        // Act
        var rates = SUT.GetTaxRates(TestData.NovaScotia, effectiveDate);

        // Assert
        Assert.Single(rates);
        Assert.Equal("NS-HST", rates.Single().Code);
        Assert.Equal(0.15m, rates.Single().Rate); // 15% HST
    }

    [Fact]
    public void GetTaxRates_ShouldReturnCorrectRates_ForBritishColumbia()
    {
        // Arrange
        var effectiveDate = new DateOnly(2023, 6, 1);

        // Act
        var rates = SUT.GetTaxRates(TestData.BritishColumbia, effectiveDate);

        // Assert
        Assert.Equal(2, rates.Count()); // GST + PST

        var gst = rates.First(r => r.Code == "GST");
        var pst = rates.First(r => r.Code == "BC-PST");

        Assert.Equal(0.05m, gst.Rate); // 5% GST
        Assert.Equal(0.07m, pst.Rate); // 7% PST
    }

    [Theory]
    [InlineData("2023-06-01", 0.15)] // Current rate
    [InlineData("2025-05-01", 0.14)] // Future reduced rate
    public void GetTaxRates_ShouldReturnCorrectHistoricalRates_ForNovaScotia(String dateString, Decimal expectedRate)
    {
        // Arrange
        var effectiveDate = DateOnly.Parse(dateString);

        // Act
        var rates = SUT.GetTaxRates(TestData.NovaScotia, effectiveDate);

        // Assert
        Assert.Single(rates);
        Assert.Equal(expectedRate, rates.Single().Rate);
    }

    [Theory]
    [InlineData("AB", 1)] // Alberta - GST only
    [InlineData("BC", 2)] // BC - GST + PST  
    [InlineData("ON", 1)] // Ontario - HST only
    [InlineData("QC", 2)] // Quebec - GST + QST
    [InlineData("NT", 1)] // Northwest Territories - GST only
    [InlineData("NU", 1)] // Nunavut - GST only
    [InlineData("YT", 1)] // Yukon - GST only
    public void GetTaxRates_ShouldReturnCorrectNumberOfTaxes_ByProvince(String provinceCode, Int32 expectedCount)
    {
        // Arrange
        var province = TestData.ProvinceManager.GetProvince(provinceCode);
        var effectiveDate = new DateOnly(2023, 6, 1);

        // Act
        var rates = SUT.GetTaxRates(province, effectiveDate);

        // Assert
        Assert.Equal(expectedCount, rates.Count());
    }

    [Theory]
    [InlineData("AB")] // Alberta - GST only
    [InlineData("NT")] // Northwest Territories - GST only  
    [InlineData("NU")] // Nunavut - GST only
    [InlineData("YT")] // Yukon - GST only
    public void GetTaxRates_ShouldReturnGSTOnly_ForGSTOnlyProvinces(String provinceCode)
    {
        // Arrange
        var province = TestData.ProvinceManager.GetProvince(provinceCode);
        var effectiveDate = new DateOnly(2023, 6, 1);

        // Act
        var rates = SUT.GetTaxRates(province, effectiveDate);

        // Assert
        Assert.Single(rates);
        Assert.Equal("GST", rates.Single().Code);
        Assert.Equal(0.05m, rates.Single().Rate);
    }

    [Fact]
    public void TaxCodeSystem_ConceptDemo_ShouldClassifyBasicGroceries_AsZeroRated()
    {
        // This test demonstrates the concept for future tax code implementation
        // Basic groceries should be zero-rated (0% but still tracked)

        // For now, we simulate this with existing structure
        var profile = new CustomerTaxProfile()
        {
            EffectiveDate = EffectiveDate,
            TaxProvince = TestData.BritishColumbia,
        };

        var invoice = new Invoice { Province = TestData.BritishColumbia };
        var invoiceItem = TestData.CreateInvoiceItem(TestData.NonTaxableProductId, EffectiveDate);
        invoice.AddItem(invoiceItem);

        var taxRates = TestData.InvoiceManager.GetTaxRatesForItem(invoiceItem, profile, invoice.InvoiceDate);

        invoiceItem.ApplyTaxes(taxRates);

        // Should have no tax (simulating zero-rated for now)
        Assert.Equal(0m, invoiceItem.GetTaxTotal());
        Assert.Equal(25.00m, invoiceItem.GetTotal());

        // Future enhancement: This would track as zero-rated rather than non-taxable
        // to support proper GST/HST compliance reporting
    }

    [Fact]
    public void TaxCodeSystem_ConceptDemo_ShouldHandleCustomerExemptions()
    {
        // This test demonstrates customer-level exemptions concept
        var regular = new CustomerTaxProfile()
        {
            EffectiveDate = EffectiveDate,
            TaxProvince = TestData.BritishColumbia,
            RecipientStatus = RecipientStatus.Regular
        };

        var exempt = new CustomerTaxProfile()
        {
            EffectiveDate = EffectiveDate,
            TaxProvince = TestData.BritishColumbia,
            RecipientStatus = RecipientStatus.TaxExempt
        };

        var invoice = new Invoice { Province = TestData.BritishColumbia };
        var invoiceItem = TestData.CreateInvoiceItem(TestData.TaxableProductId, EffectiveDate);
        invoice.AddItem(invoiceItem);

        // Test 1: Regular customer gets full tax
        var regularTaxRates = TestData.InvoiceManager.GetTaxRatesForItem(invoiceItem, regular, invoice.InvoiceDate);

        invoiceItem.ApplyTaxes(regularTaxRates);
        var regularTax = invoiceItem.GetTaxTotal();

        // Reset for exempt customer test
        invoiceItem.AppliedTaxes.Clear();

        // Test 2: Exempt customer gets no tax (simulated by applying no rates)
        var exemptTaxRates = TestData.InvoiceManager.GetTaxRatesForItem(invoiceItem, exempt, EffectiveDate); // Simulating exemption
        invoiceItem.ApplyTaxes(exemptTaxRates);
        var exemptTax = invoiceItem.GetTaxTotal();

        Assert.True(regularTax > 0);
        Assert.Equal(0m, exemptTax);

        // Future enhancement: This would be based on CustomerTaxProfile
        // with proper exemption certificate validation
    }
}

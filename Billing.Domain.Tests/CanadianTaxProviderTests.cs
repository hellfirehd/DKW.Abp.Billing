namespace Billing.Domain.Tests;

/// <summary>
/// Unit tests for the Canadian Tax Provider
/// </summary>
public class CanadianTaxProviderTests
{
    private readonly CanadianTaxProvider _taxProvider = new();

    [Fact]
    public void GetTaxRates_ShouldReturnCorrectRates_ForNovaScotia()
    {
        // Arrange
        var effectiveDate = new DateOnly(2023, 6, 1);

        // Act
        var rates = _taxProvider.GetTaxRates(Provinces.NS, effectiveDate);

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
        var rates = _taxProvider.GetTaxRates(Provinces.BC, effectiveDate);

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
        var rates = _taxProvider.GetTaxRatesForCategory(Provinces.BC, effectiveDate, TaxCategory.NonTaxableProduct);

        // Assert
        Assert.Empty(rates);
    }

    [Fact]
    public void HasTaxConfiguration_ShouldReturnTrue_ForAllCanadianProvinces()
    {
        // Act & Assert
        Assert.True(_taxProvider.HasTaxConfiguration(Provinces.AB));
        Assert.True(_taxProvider.HasTaxConfiguration(Provinces.BC));
        Assert.True(_taxProvider.HasTaxConfiguration(Provinces.MB));
        Assert.True(_taxProvider.HasTaxConfiguration(Provinces.NB));
        Assert.True(_taxProvider.HasTaxConfiguration(Provinces.NL));
        Assert.True(_taxProvider.HasTaxConfiguration(Provinces.NS));
        Assert.True(_taxProvider.HasTaxConfiguration(Provinces.ON));
        Assert.True(_taxProvider.HasTaxConfiguration(Provinces.PE));
        Assert.True(_taxProvider.HasTaxConfiguration(Provinces.QC));
        Assert.True(_taxProvider.HasTaxConfiguration(Provinces.SK));
        Assert.True(_taxProvider.HasTaxConfiguration(Provinces.NT));
        Assert.True(_taxProvider.HasTaxConfiguration(Provinces.NU));
        Assert.True(_taxProvider.HasTaxConfiguration(Provinces.YT));
    }

    [Fact]
    public void GetSupportedProvinces_ShouldReturnAllCanadianProvinces()
    {
        // Act
        var provinces = _taxProvider.GetSupportedProvinces();

        // Assert
        Assert.Equal(13, provinces.Length); // All Canadian provinces and territories
        Assert.Contains(Provinces.AB, provinces);
        Assert.Contains(Provinces.BC, provinces);
        Assert.Contains(Provinces.NS, provinces);
        Assert.Contains(Provinces.ON, provinces);
        Assert.Contains(Provinces.QC, provinces);
    }

    [Theory]
    [InlineData("2023-06-01", 0.15)] // Current rate
    [InlineData("2025-05-01", 0.14)] // Future reduced rate
    public void GetTaxRates_ShouldReturnCorrectHistoricalRates_ForNovaScotia(string dateString, decimal expectedRate)
    {
        // Arrange
        var effectiveDate = DateOnly.Parse(dateString);

        // Act
        var rates = _taxProvider.GetTaxRates(Provinces.NS, effectiveDate);

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
        var province = Provinces.All.First(p => p.Code == provinceCode);
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
        var province = Province.Create(provinceCode, provinceCode);
        var effectiveDate = new DateOnly(2023, 6, 1);

        // Act
        var rates = _taxProvider.GetTaxRates(province, effectiveDate);

        // Assert
        Assert.Single(rates);
        Assert.Equal("GST", rates[0].Code);
        Assert.Equal(0.05m, rates[0].TaxRate.Rate);
    }

    [Fact]
    public void CanadianTaxProvider_ShouldCalculateCorrectTotalTax_ForMixedItems()
    {
        // Arrange
        var invoice = new Invoice { Province = Provinces.BC }; // BC: 5% GST + 7% PST = 12%

        var taxableProduct = new Product
        {
            Description = "Standard Product",
            UnitPrice = 100.00m,
            Quantity = 1,
            TaxCategory = TaxCategory.TaxableProduct
        };

        var nonTaxableProduct = new Product
        {
            Description = "Non-Taxable Product",
            UnitPrice = 50.00m,
            Quantity = 1,
            TaxCategory = TaxCategory.NonTaxableProduct
        };

        invoice.AddItem(taxableProduct);
        invoice.AddItem(nonTaxableProduct);

        // Act
        var allTaxRates = _taxProvider.GetTaxRates(invoice.Province, invoice.InvoiceDate);
        invoice.SetTaxRates(allTaxRates);

        // Assert
        // Only the taxable product should have tax: $100 * 12% = $12
        Assert.Equal(12.00m, invoice.GetTotalTax());
        Assert.Equal(2, taxableProduct.AppliedTaxes.Count); // GST + PST
        Assert.Empty(nonTaxableProduct.AppliedTaxes); // No taxes
        Assert.Equal(162.00m, invoice.GetTotal()); // $150 subtotal + $12 tax
    }

    [Fact]
    public void CanadianTaxProvider_ShouldHandleComplexCalculations()
    {
        // Create a comprehensive test invoice
        var invoice = new Invoice
        {
            Province = Provinces.ON, // 13% HST
            ShippingCost = 15.00m
        };

        // Add multiple items with different tax treatments
        var software = new Product
        {
            Description = "Software License",
            UnitPrice = 200.00m,
            Quantity = 2,
            TaxCategory = TaxCategory.DigitalProduct
        };

        var consulting = new Service
        {
            Description = "Professional Consulting",
            HourlyRate = 150.00m,
            Hours = 4.0m,
            TaxCategory = TaxCategory.TaxableService
        };

        invoice.AddItem(software);
        invoice.AddItem(consulting);

        // Apply volume discount
        var discount = new Discount
        {
            Name = "Volume Discount",
            Type = DiscountType.Percentage,
            Scope = DiscountScope.PerOrder,
            Value = 10.0m, // 10%
            IsActive = true
        };
        invoice.AddOrderDiscount(discount);

        // Apply credit card surcharge
        var surcharge = new Surcharge
        {
            Name = "Credit Card Processing",
            FixedAmount = 2.50m,
            PercentageRate = 0.029m, // 2.9%
            IsActive = true
        };
        invoice.AddSurcharge(surcharge);

        // Apply taxes
        var taxRates = _taxProvider.GetTaxRates(invoice.Province, invoice.InvoiceDate);
        invoice.SetTaxRates(taxRates);

        // Verify calculation chain
        var subtotal = invoice.GetSubtotal(); // (200*2) + (150*4) = 1000
        var totalAfterDiscounts = invoice.GetTotalAfterDiscounts(); // 1000 - 100 + 15 = 915
        var totalTax = invoice.GetTotalTax(); // 915 * 0.13 = 119.95
        var totalSurcharges = invoice.GetTotalSurcharges(); // Based on 915 + 119.95
        var finalTotal = invoice.GetTotal();

        Assert.Equal(1000.00m, subtotal);
        Assert.Equal(915.00m, totalAfterDiscounts);
        Assert.Equal(119.95m, totalTax);
        Assert.True(totalSurcharges > 0);
        Assert.True(finalTotal > 1000.00m);
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

        var invoice = new Invoice { Province = Provinces.BC };
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

        var invoice = new Invoice { Province = Provinces.BC };
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

// Add these new test methods to the existing InvoiceTests.cs file
public partial class InvoiceTests
{
    [Fact]
    public void SetTaxRates_ShouldApplyCorrectTaxes_ByCategory()
    {
        // Arrange
        var taxProvider = new CanadianTaxProvider();
        var invoice = new Invoice { Province = Provinces.BC };

        var taxableProduct = new Product
        {
            Description = "Taxable Product",
            UnitPrice = 100.00m,
            Quantity = 1,
            TaxCategory = TaxCategory.TaxableProduct
        };

        var taxableService = new Service
        {
            Description = "Taxable Service",
            HourlyRate = 100.00m,
            Hours = 1.0m,
            TaxCategory = TaxCategory.TaxableService
        };

        var nonTaxableProduct = new Product
        {
            Description = "Non-Taxable Product",
            UnitPrice = 50.00m,
            Quantity = 1,
            TaxCategory = TaxCategory.NonTaxableProduct
        };

        invoice.AddItem(taxableProduct);
        invoice.AddItem(taxableService);
        invoice.AddItem(nonTaxableProduct);

        // Act
        var allTaxRates = taxProvider.GetTaxRates(invoice.Province, invoice.InvoiceDate);
        invoice.SetTaxRates(allTaxRates);

        // Assert
        Assert.Equal(24.00m, invoice.GetTotalTax()); // (100 + 100) * 0.12 = 24.00 (5% GST + 7% PST)
        Assert.Equal(2, taxableProduct.AppliedTaxes.Count); // GST + PST
        Assert.Equal(2, taxableService.AppliedTaxes.Count); // GST + PST
        Assert.Empty(nonTaxableProduct.AppliedTaxes); // No taxes
    }

    [Fact]
    public void TaxCalculation_ShouldBeAccurate_ForComplexScenario()
    {
        // Arrange
        var taxProvider = new CanadianTaxProvider();
        var invoice = new Invoice
        {
            Province = Provinces.ON, // 13% HST
            ShippingCost = 10.00m
        };

        var product = new Product
        {
            Description = "Software License",
            UnitPrice = 199.99m,
            Quantity = 2,
            TaxCategory = TaxCategory.TaxableProduct
        };

        var service = new Service
        {
            Description = "Support Service",
            HourlyRate = 50.00m,
            Hours = 3.0m,
            TaxCategory = TaxCategory.TaxableService
        };

        // Add line item discount
        var itemDiscount = new Discount
        {
            Name = "Volume Discount",
            Type = DiscountType.Percentage,
            Scope = DiscountScope.PerItem,
            Value = 10.0m, // 10% off
            IsActive = true
        };

        product.ApplyDiscounts([itemDiscount]);

        invoice.AddItem(product);
        invoice.AddItem(service);

        // Add order discount
        var orderDiscount = new Discount
        {
            Name = "Early Bird Discount",
            Type = DiscountType.FixedAmount,
            Scope = DiscountScope.PerOrder,
            Value = 25.00m,
            IsActive = true
        };
        invoice.AddOrderDiscount(orderDiscount);

        // Add surcharge
        var surcharge = new Surcharge
        {
            Name = "Payment Processing",
            FixedAmount = 2.00m,
            PercentageRate = 0.029m, // 2.9%
            IsActive = true
        };
        invoice.AddSurcharge(surcharge);

        // Apply taxes
        var taxRates = taxProvider.GetTaxRates(invoice.Province, invoice.InvoiceDate);
        invoice.SetTaxRates(taxRates);

        // Act & Assert
        var subtotal = invoice.GetSubtotal(); // (199.99 * 2) + (50 * 3) = 549.98
        var itemDiscounts = invoice.GetLineItemDiscounts(); // 199.99 * 2 * 0.10 = 40.00 (rounded)
        var orderDiscounts = invoice.GetOrderDiscounts(); // 25.00
        var totalAfterDiscounts = invoice.GetTotalAfterDiscounts(); // 549.98 - 40.00 - 25.00 + 10.00 = 494.98
        var totalTax = invoice.GetTotalTax(); // Should be calculated on discounted amounts
        var totalSurcharges = invoice.GetTotalSurcharges();
        var finalTotal = invoice.GetTotal();

        // Verify the calculation chain
        Assert.Equal(549.98m, subtotal);
        Assert.True(itemDiscounts > 0);
        Assert.Equal(25.00m, orderDiscounts);
        Assert.True(totalTax > 0);
        Assert.True(totalSurcharges > 0);
        Assert.True(finalTotal > totalAfterDiscounts);
    }
}
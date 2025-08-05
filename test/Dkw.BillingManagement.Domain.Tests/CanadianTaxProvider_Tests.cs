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
using Dkw.BillingManagement.Taxes;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace Dkw.BillingManagement;

/// <summary>
/// Unit tests for the Canadian Tax Provider
/// </summary>
public abstract class CanadianTaxProvider_Tests<TStartupModule> : BillingManagemenDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private static readonly DateOnly EffectiveDate = new(2025, 01, 01);

    public CanadianTaxProvider SUT { get; }

    protected CanadianTaxProvider_Tests()
    {
        SUT = GetRequiredService<CanadianTaxProvider>();
    }

    protected override void BeforeAddApplication(IServiceCollection services)
    {
        services.AddTransient<CanadianTaxProvider>();
    }

    [Fact]
    public async Task GetTaxRates_ShouldReturnCorrectRates_ForNovaScotia()
    {
        // Arrange
        var effectiveDate = new DateOnly(2023, 6, 1);
        var province = await ProvinceRepository.GetAsync("NS");

        // Act
        var rates = await SUT.GetApplicableTaxesAsync(province, effectiveDate);

        // Assert
        Assert.Single(rates);
        Assert.Equal("NS-HST", rates.Single().Code);
        Assert.Equal(0.15m, rates.Single().Rate); // 15% HST
    }

    [Fact]
    public async Task GetTaxRates_ShouldReturnCorrectRates_ForBritishColumbia()
    {
        // Arrange
        var effectiveDate = new DateOnly(2023, 6, 1);
        var province = await ProvinceRepository.GetAsync("BC");

        // Act
        var rates = await SUT.GetApplicableTaxesAsync(province, effectiveDate);

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
    public async Task GetTaxRates_ShouldReturnCorrectHistoricalRates_ForNovaScotia(String dateString, Decimal expectedRate)
    {
        // Arrange
        var province = await ProvinceRepository.GetAsync("NS");
        var effectiveDate = DateOnly.Parse(dateString, CultureInfo.InvariantCulture);

        // Act
        var rates = await SUT.GetApplicableTaxesAsync(province, effectiveDate);

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
    public async Task GetTaxRates_ShouldReturnCorrectNumberOfTaxes_ByProvince(String provinceCode, Int32 expectedCount)
    {
        // Arrange
        var province = await ProvinceRepository.GetAsync(provinceCode);
        var effectiveDate = new DateOnly(2023, 6, 1);

        // Act
        var rates = await SUT.GetApplicableTaxesAsync(province, effectiveDate);

        // Assert
        Assert.Equal(expectedCount, rates.Count());
    }

    [Theory]
    [InlineData("AB")] // Alberta - GST only
    [InlineData("NT")] // Northwest Territories - GST only  
    [InlineData("NU")] // Nunavut - GST only
    [InlineData("YT")] // Yukon - GST only
    public async Task GetTaxRates_ShouldReturnGSTOnly_ForGSTOnlyProvinces(String provinceCode)
    {
        // Arrange
        var province = await ProvinceRepository.GetAsync(provinceCode);
        var effectiveDate = new DateOnly(2023, 6, 1);

        // Act
        var rates = await SUT.GetApplicableTaxesAsync(province, effectiveDate);

        // Assert
        Assert.Single(rates);
        Assert.Equal("GST", rates.Single().Code);
        Assert.Equal(0.05m, rates.Single().Rate);
    }

    [Fact]
    public async Task TaxCodeSystem_ConceptDemo_ShouldClassifyBasicGroceries_AsZeroRated()
    {
        // This test demonstrates the concept for future tax code implementation
        // Basic groceries should be zero-rated (0% but still tracked)

        var province = await ProvinceRepository.GetAsync("BC");

        var profile = new CustomerTaxProfile()
        {
            EffectiveDate = EffectiveDate,
            TaxProvince = province,
        };

        var invoice = TestData.EmptyInvoice(province);
        var invoiceItem = await NonTaxableProductItemAsync(EffectiveDate);
        invoice.AddLineItem(invoiceItem);

        var taxRates = await InvoiceManager.GetApplicableTaxesAsync(invoiceItem, profile, invoice.InvoiceDate);

        invoiceItem.ApplyTaxes(taxRates);

        // Should have no tax (simulating zero-rated for now)
        Assert.Equal(0m, invoiceItem.GetTaxTotal());
        Assert.Equal(50.00m, invoiceItem.GetTotal());

        // Future enhancement: This would track as zero-rated rather than non-taxable
        // to support proper GST/HST compliance reporting
    }

    [Fact]
    public async Task TaxCodeSystem_ConceptDemo_ShouldHandleCustomerExemptions()
    {
        // This test demonstrates customer-level exemptions concept
        var province = await ProvinceRepository.GetAsync("BC");
        var regular = new CustomerTaxProfile()
        {
            EffectiveDate = EffectiveDate,
            TaxProvince = province,
            RecipientStatus = CustomerTaxType.Regular
        };

        var exempt = new CustomerTaxProfile()
        {
            EffectiveDate = EffectiveDate,
            TaxProvince = province,
            RecipientStatus = CustomerTaxType.TaxExempt
        };

        var invoice = TestData.EmptyInvoice(province);
        var invoiceItem = await TaxableProductItemAsync(EffectiveDate);
        invoice.AddLineItem(invoiceItem);

        // Test 1: Regular customer gets full tax
        var regularTaxRates = await InvoiceManager.GetApplicableTaxesAsync(invoiceItem, regular, invoice.InvoiceDate);

        invoiceItem.ApplyTaxes(regularTaxRates);
        var regularTax = invoiceItem.GetTaxTotal();

        // Test 2: Exempt customer gets no tax (simulated by applying no rates)
        var exemptTaxRates = await InvoiceManager.GetApplicableTaxesAsync(invoiceItem, exempt, EffectiveDate); // Simulating exemption
        invoiceItem.ApplyTaxes(exemptTaxRates);
        var exemptTax = invoiceItem.GetTaxTotal();

        Assert.True(regularTax > 0);
        Assert.Equal(0m, exemptTax);

        // Future enhancement: This would be based on CustomerTaxProfile
        // with proper exemption certificate validation
    }
}

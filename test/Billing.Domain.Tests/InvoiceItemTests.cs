using Dkw.Abp.Billing.EntityFrameworkCore;

namespace Dkw.Abp.Billing;

public class InvoiceItemTests
{
    private static readonly DateOnly EffectiveDate = new(2025, 01, 01);
    private readonly FakeTaxCodeRepository _tcr = new();

    [Fact]
    public void InvoiceItem_ShouldCalculateTotal_WithTax()
    {
        // Arrange
        var tax = new Tax("GST", "GST", TaxJurisdiction.Federal)
            .AddTaxRate(0.05m, new DateOnly(2000, 01, 01)); // 5% GST

        var item = TestData.CreateInvoiceItem(TestData.TaxableProductId, EffectiveDate);

        item.ApplyTaxes([tax.GetTaxRate(EffectiveDate)!]);

        // Act
        var total = item.GetTotal();

        // Assert
        Assert.Equal(105.00m, total); // (2 * 50) + (2 * 50 * 0.05)
    }

    [Fact]
    public void InvoiceItem_ShouldCalculateTotal_WithoutApplyingTaxes()
    {
        // Arrange
        var item = TestData.CreateInvoiceItem(TestData.NonTaxableProductId, EffectiveDate);

        // Act
        var total = item.GetTotal();

        // Assert
        Assert.Equal(100.00m, total); // 2 * 50
    }

    [Fact]
    public void InvoiceItem_ShouldCalculateTotal_WithMultipleTaxes()
    {
        // Arrange
        var gst = new Tax("GST", "GST", TaxJurisdiction.Federal)
            .AddTaxRate(0.05m, new DateOnly(2000, 01, 01)); // 5% GST
        var pst = new Tax("PST", "PST", TaxJurisdiction.Provincial)
            .AddTaxRate(0.07m, new DateOnly(2000, 01, 01)); // 7% PST
        var item = TestData.CreateInvoiceItem(TestData.TaxableProductId, EffectiveDate);
        item.ApplyTaxes([gst.GetTaxRate(EffectiveDate)!, pst.GetTaxRate(EffectiveDate)!]);

        // Act
        var total = item.GetTotal();

        // Assert
        Assert.Equal(114.00m, total); // (2 * 50) + (2 * 50 * 0.05) + (2 * 50 * 0.07)
    }

    [Fact]
    public void InvoiceItem_ShouldCalculateTotal_WithExemptions()
    {
        // Arrange
        var tax = new Tax("GST", "GST", TaxJurisdiction.Federal).AddTaxRate(0.05m, new DateOnly(2000, 01, 01)); // 5% GST

        var item = TestData.CreateInvoiceItem(TestData.NonTaxableProductId, EffectiveDate);
        item.ApplyTaxes([tax.GetTaxRate(EffectiveDate)!]);

        // Act
        var total = item.GetTotal();

        // Assert
        Assert.Equal(100.00m, total); // No tax applied due to exemption
    }

    [Fact]
    public void InvoiceItem_ShouldSetTheItemType_TheSameAsTheOriginalItem()
    {
        // Arrange
        var item = TestData.TaxableProduct(TestData.TaxableProductId);

        // Act
        var invoiceItem = TestData.CreateInvoiceItem(TestData.TaxableProductId, EffectiveDate);

        // Assert
        Assert.Equal(item.ItemType, invoiceItem.ItemType);
    }
}

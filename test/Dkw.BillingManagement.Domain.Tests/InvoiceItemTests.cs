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

using Dkw.BillingManagement.EntityFrameworkCore;

namespace Dkw.BillingManagement;

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

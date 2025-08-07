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

using Dkw.BillingManagement.Taxes;
using Volo.Abp.Modularity;

namespace Dkw.BillingManagement.Invoices.LineItems;

public abstract class LineItem_Tests<TStartupModule> : BillingManagementDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private static readonly DateOnly EffectiveDate = new(2025, 01, 01);

    [Fact]
    public async Task LineItem_ShouldCalculateTotal_WithMultipleTaxes()
    {
        // Arrange
        var lineItem = await NonTaxableProductItemAsync(EffectiveDate);
        lineItem.ChangeQuantity(2);

        var gst = new Tax(Guid.NewGuid(), "GST", "GST")
            .AddTaxRate(0.05m, new DateOnly(2000, 01, 01)); // 5% GST

        var pst = new Tax(Guid.NewGuid(), "PST", "PST")
            .AddTaxRate(0.07m, new DateOnly(2000, 01, 01)); // 7% PST

        lineItem.ApplyTaxes([gst.GetTaxRate(EffectiveDate)!, pst.GetTaxRate(EffectiveDate)!]);

        // Act
        var total = lineItem.GetTotal();

        // Assert
        Assert.Equal(112.00m, total); // (2 * 50) + (2 * 50 * 0.05) + (2 * 50 * 0.07) == 
    }

    [Fact]
    public async Task LineItem_ShouldCalculateTotal_WithoutApplyingTaxes()
    {
        // Arrange
        var lineItem = await NonTaxableProductItemAsync(EffectiveDate);
        lineItem.ChangeQuantity(2);

        // Act
        var total = lineItem.GetTotal();

        // Assert
        Assert.Equal(100.00m, total); // 2 * 50
    }

    [Fact]
    public async Task LineItem_ShouldCalculateTotal_WithTax()
    {
        // Arrange
        var tax = new Tax(Guid.NewGuid(), "GST", "GST")
            .AddTaxRate(0.05m, new DateOnly(2000, 01, 01)); // 5% GST

        var lineItem = await TaxableProductItemAsync(EffectiveDate); // @ $100.00 ea
        lineItem.ChangeQuantity(2);
        lineItem.ApplyTaxes([tax.GetTaxRate(EffectiveDate)!]);

        // Act
        var total = lineItem.GetTotal();

        // Assert
        Assert.Equal(210.00m, total); // (2 * 100) + (2 * 100 * 0.05)
    }
}

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

using Dkw.BillingManagement.Invoices.LineItems;
using Dkw.BillingManagement.Items;
using Dkw.BillingManagement.Payments;
using Dkw.BillingManagement.Provinces;
using Shouldly;
using Volo.Abp.Modularity;

namespace Dkw.BillingManagement.Invoices;

/// <summary>
/// Unit tests for the Invoice domain entity
/// </summary>
public abstract class Invoice_Tests<TStartupModule> : BillingManagementDomainTestBase<TStartupModule>, IAsyncLifetime
    where TStartupModule : IAbpModule
{
    private static readonly DateOnly EffectiveDate = new(2025, 01, 01);
    protected Province Province { get; set; } = Province.Empty;

    protected Invoice Invoice { get; set; } = default!;

    public async Task InitializeAsync()
    {
        Province = await ProvinceRepository.GetAsync("ON");
        Invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SetTaxRates_ShouldApplyCorrectTaxes_ByCategory()
    {
        // Arrange
        var province = await ProvinceRepository.GetAsync("BC");

        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(province), province, EffectiveDate);

        var taxableProduct = await TaxableProductItemAsync(EffectiveDate);
        invoice.AddLineItem(taxableProduct);

        var taxableService = await TaxableServiceItemAsync(EffectiveDate);
        invoice.AddLineItem(taxableService);

        var nonTaxableProduct = await NonTaxableProductItemAsync(EffectiveDate);
        invoice.AddLineItem(nonTaxableProduct);

        // Act
        var allTaxRates = await TaxProvider.GetTaxesAsync(invoice.PlaceOfSupply, invoice.InvoiceDate);
        invoice.ApplyTaxes(allTaxRates);

        // Assert
        Assert.Equal(24.00m, invoice.GetTaxAmount()); // (100 + 100) * 0.12 = 24.00 (5% GST + 7% PST)
        Assert.Equal(2, taxableProduct.AppliedTaxes.Count); // GST + PST
        Assert.Equal(2, taxableService.AppliedTaxes.Count); // GST + PST
        Assert.Empty(nonTaxableProduct.AppliedTaxes); // No taxes
    }

    [Fact]
    public async Task Invoice_ShouldHandleComplexCalculations()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);

        // Add multiple items with different tax treatments
        var software = await TaxableProductItemAsync(EffectiveDate);
        invoice.AddLineItem(software);

        var consulting = await NonTaxableProductItemAsync(EffectiveDate);
        invoice.AddLineItem(consulting);

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
        var taxRates = await TaxProvider.GetTaxesAsync(invoice.PlaceOfSupply, invoice.InvoiceDate);
        invoice.ApplyTaxes(taxRates);

        // Verify calculation chain
        var subtotal = invoice.GetSubtotal(); // (500*2) + (150*4) = 1000
        var totalAfterDiscounts = invoice.GetTotalAmount(); // 1000 - 100 + 15 = 915
        var totalTax = invoice.GetTaxAmount(); // 915 * 0.13 = 119.95
        var totalSurcharges = invoice.GetSurchargeAmount(); // Based on 915 + 119.95
        var finalTotal = invoice.GetGrandTotal();

        Assert.Equal(1000.00m, subtotal);
        Assert.Equal(915.00m, totalAfterDiscounts);
        Assert.Equal(119.95m, totalTax);
        Assert.True(totalSurcharges > 0);
        Assert.True(finalTotal > 1000.00m);
    }

    [Fact]
    public async Task Invoice_ShouldCalculateCorrectTotalTax_ForMixedItems()
    {
        // Arrange
        var province = await ProvinceRepository.GetAsync("BC");
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(province), province, EffectiveDate);

        var taxableProduct = await TaxableProductItemAsync(EffectiveDate);
        invoice.AddLineItem(taxableProduct);

        var nonTaxableProduct = await NonTaxableProductItemAsync(EffectiveDate);
        invoice.AddLineItem(nonTaxableProduct);

        var allTaxRates = await TaxProvider.GetTaxesAsync(invoice.PlaceOfSupply, invoice.InvoiceDate);

        // Act
        invoice.ApplyTaxes(allTaxRates);

        // Assert
        // Only the taxable product should have tax: $100 * 12% = $12
        Assert.Equal(12.00m, invoice.GetTaxAmount());
        Assert.Equal(2, taxableProduct.AppliedTaxes.Count); // GST + PST
        Assert.Empty(nonTaxableProduct.AppliedTaxes); // No taxes
        Assert.Equal(162.00m, invoice.GetGrandTotal()); // $150 subtotal + $12 tax
    }

    [Fact]
    public async Task TaxCalculation_ShouldBeAccurate_ForComplexScenario()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);

        var product = await TaxableProductItemAsync(EffectiveDate);

        var service = await TaxableServiceItemAsync(EffectiveDate);

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

        invoice.AddLineItem(product);
        invoice.AddLineItem(service);

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
        var taxRates = await TaxProvider.GetTaxesAsync(invoice.PlaceOfSupply, invoice.InvoiceDate);
        invoice.ApplyTaxes(taxRates);

        // Act & Assert
        var subtotal = invoice.GetSubtotal(); // (199.99 * 2) + (50 * 3) = 549.98
        var itemDiscounts = invoice.GetLineItemDiscountAmount(); // 199.99 * 2 * 0.10 = 40.00 (rounded)
        var orderDiscounts = invoice.GetOrderDiscountAmount(); // 25.00
        var totalAfterDiscounts = invoice.GetTotalAmount(); // 549.98 - 40.00 - 25.00 + 10.00 = 494.98
        var totalTax = invoice.GetTaxAmount(); // Should be calculated on discounted amounts
        var totalSurcharges = invoice.GetSurchargeAmount();
        var finalTotal = invoice.GetGrandTotal();

        // Verify the calculation chain
        Assert.Equal(549.98m, subtotal);
        Assert.True(itemDiscounts > 0);
        Assert.Equal(25.00m, orderDiscounts);
        Assert.True(totalTax > 0);
        Assert.True(totalSurcharges > 0);
        Assert.True(finalTotal > totalAfterDiscounts);
    }

    [Fact]
    public async Task Invoice_AddItem_ShouldAddItemAndUpdateSubtotal()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
        var product = await TaxableProductItemAsync(EffectiveDate);

        // Act
        invoice.AddLineItem(product);

        // Assert
        Assert.Single(invoice.LineItems);
        Assert.Equal(product, invoice.LineItems[0]);
        Assert.Equal(100.00m, invoice.GetSubtotal());
    }

    [Fact]
    public async Task Invoice_RemoveItem_ShouldRemoveItemAndUpdateSubtotal()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
        var product = await TaxableProductItemAsync(EffectiveDate);
        invoice.AddLineItem(product);

        // Act
        invoice.RemoveLineItem(product.Id);

        // Assert
        Assert.Empty(invoice.LineItems);
        Assert.Equal(0m, invoice.GetSubtotal());
    }

    [Fact]
    public async Task Invoice_AddOrderDiscount_ShouldApplyDiscountToTotal()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
        invoice.AddLineItem(await TaxableProductItemAsync(EffectiveDate));

        var orderDiscount = new Discount
        {
            Name = "Order Discount",
            Type = DiscountType.FixedAmount,
            Scope = DiscountScope.PerOrder,
            Value = 50.00m,
            IsActive = true
        };

        // Act
        invoice.AddOrderDiscount(orderDiscount);

        // Assert
        Assert.Single(invoice.Discounts);
        Assert.Equal(50.00m, invoice.GetOrderDiscountAmount());
    }

    [Fact]
    public async Task Invoice_AddDiscount_ShouldApplyDiscountToTotal()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
        invoice.AddLineItem(await TaxableProductItemAsync(EffectiveDate)); // $100.00

        var discount = new Discount
        {
            Name = "Order Discount",
            Type = DiscountType.FixedAmount,
            Scope = DiscountScope.PerOrder,
            Value = 50.00m,
            IsActive = true
        };

        // Act
        invoice.AddOrderDiscount(discount);

        // Assert
        Assert.Single(invoice.Discounts);
        Assert.Equal(100, invoice.GetSubtotal());
        Assert.Equal(50.00m, invoice.GetOrderDiscountAmount());
        Assert.Equal(50.00m, invoice.GetTotalAmount());
    }

    [Fact]
    public async Task Invoice_AddSurcharge_ShouldApplySurchargeToTotal()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
        invoice.AddLineItem(await TaxableProductItemAsync(EffectiveDate));

        var surcharge = new Surcharge
        {
            Name = "Credit Card Fee",
            FixedAmount = 0.30m,
            PercentageRate = 0.029m, // 2.9%
            IsActive = true
        };

        // Act
        invoice.AddSurcharge(surcharge);

        // Assert
        Assert.Single(invoice.Surcharges);
        // Surcharge = 0.30 + (100 * 0.029) = 0.30 + 2.90 = 3.20
        Assert.Equal(3.20m, invoice.GetSurchargeAmount());
    }

    [Fact]
    public async Task Invoice_AddPayment_ShouldUpdateStatusAndBalance()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
        invoice.AddLineItem(await TaxableProductItemAsync(EffectiveDate));

        var payment = new Payment(100.00m, EffectiveDate, TestData.CreditCard(), String.Empty)
        {
            Status = PaymentStatus.Completed
        };
        payment.MarkAsCompleted();

        // Act
        invoice.ProcessPayment(payment);

        // Assert
        Assert.Single(invoice.Payments);
        Assert.Equal(100.00m, invoice.GetTotalPaid());
        Assert.Equal(0m, invoice.GetBalance());
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    [Fact]
    public async Task Invoice_ProcessRefund_ShouldUpdateStatusAndBalance()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
        invoice.AddLineItem(await TaxableProductItemAsync(EffectiveDate));

        var payment = new Payment(100.00m, EffectiveDate, TestData.CreditCard(), String.Empty)
        {
            Status = PaymentStatus.Completed
        };
        payment.MarkAsCompleted();
        invoice.ProcessPayment(payment);

        var refund = new Refund(Guid.NewGuid(), 25.00m, "Partial return");

        // Act
        invoice.ProcessRefund(refund);

        // Assert
        Assert.Single(invoice.Refunds);
        Assert.Equal(25.00m, invoice.GetTotalRefunded());
        Assert.Equal(25.00m, invoice.GetBalance()); // 100 total - 100 paid + 25 refunded = 25
        Assert.Equal(InvoiceStatus.PartiallyRefunded, invoice.Status);
    }

    [Fact]
    public async Task Invoice_ProcessRefund_ShouldThrowException_WhenRefundExceedsNetPayments()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
        invoice.AddLineItem(await TaxableProductItemAsync(EffectiveDate));

        var payment = new Payment(50.00m, EffectiveDate, TestData.CreditCard(), String.Empty);
        payment.MarkAsCompleted();
        invoice.ProcessPayment(payment);

        var refund = new Refund(Guid.NewGuid(), 75.00m, "Invalid refund");

        // Act & Assert
        var exception = Assert.Throws<BillingManagementException>(() => invoice.ProcessRefund(refund));

        exception.ErrorCode.ShouldBe(ErrorCodes.InvalidRefundAmount);
    }

    [Fact]
    public async Task UpdateStatus_ShouldSetOverdue_WhenPastDueDate()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate)
        { DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) };

        invoice.AddLineItem(await TaxableProductItemAsync(EffectiveDate));

        // Act
        invoice.UpdateStatus();

        // Assert
        Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
    }

    [Fact]
    public void PostInvoice_ShouldThrowException_WhenThereAreNoLineItems()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);

        // Act
        var exception = Assert.Throws<BillingManagementException>(invoice.PostInvoice);

        exception.ErrorCode.ShouldBe(ErrorCodes.CannotPostInvoice);
    }

    [Fact]
    public async Task PostInvoice_ShouldUpdateStatus_WhenInDraftOrPending()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
        invoice.AddLineItem(await TaxableProductItemAsync(EffectiveDate));

        // Act
        invoice.PostInvoice();

        // Assert
        Assert.Equal(InvoiceStatus.Posted, invoice.Status);
    }

    [Fact]
    public async Task CancelInvoice_Allowed_WhenNoPayments()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
        invoice.AddLineItem(await TaxableProductItemAsync(EffectiveDate));

        // Act
        invoice.CancelInvoice();

        // Assert
        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
    }

    [Fact]
    public async Task CancelInvoice_ShouldThrowException_WhenPaymentsExist()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
        invoice.AddLineItem(await TaxableProductItemAsync(EffectiveDate));

        var payment = new Payment(50.00m, EffectiveDate, TestData.CreditCard(), String.Empty);
        payment.MarkAsCompleted();
        invoice.ProcessPayment(payment);

        // Act & Assert
        var exception = Assert.Throws<BillingManagementException>(() => invoice.CancelInvoice());
        exception.ErrorCode.ShouldBe(ErrorCodes.CannotCancelInvoice);
    }

    [Fact]
    public async Task ComplexScenario_ShouldCalculateCorrectTotals()
    {
        // Arrange - Complex scenario with multiple items, taxes, discounts, and surcharges
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
        invoice.AddLineItem(await TaxableProductItemAsync(EffectiveDate)); // $100 taxable product
        invoice.AddLineItem(await NonTaxableProductItemAsync(EffectiveDate)); // $50 non-taxable product
        invoice.AddLineItem(await TaxableServiceItemAsync(EffectiveDate)); // $500 taxable service
        invoice.AddLineItem(await NonTaxableServiceItemAsync(EffectiveDate)); // $75 non-taxable service

        // Apply order discount for large orders
        var orderDiscount = new Discount
        {
            Name = "Large Order Discount",
            Type = DiscountType.Percentage,
            Scope = DiscountScope.PerOrder,
            Value = 0.15m, // 15% off
            IsActive = true
        };
        invoice.AddOrderDiscount(orderDiscount);

        // Add credit card surcharge
        var surcharge = new Surcharge
        {
            Name = "Credit Card Processing",
            Type = SurchargeType.CreditCardProcessing,
            FixedAmount = 0.30m,
            PercentageRate = 0.029m,
            IsActive = true
        };
        invoice.AddSurcharge(surcharge);

        // Apply taxes
        var taxes = await TaxProvider.GetTaxesAsync(invoice.PlaceOfSupply, invoice.InvoiceDate);
        invoice.ApplyTaxes(taxes);

        // Act
        var subtotal = invoice.GetSubtotal();
        var taxTotal = invoice.GetTaxAmount();
        var lineItemDiscount = invoice.GetLineItemDiscountAmount();
        var orderDiscountTotal = invoice.GetOrderDiscountAmount();
        var surchargeTotal = invoice.GetSurchargeAmount();
        var finalTotal = invoice.GetGrandTotal();

        // Assert
        // Verify calculations are reasonable
        Assert.True(subtotal > Decimal.Zero);
        Assert.True(taxTotal > Decimal.Zero);
        Assert.True(lineItemDiscount == Decimal.Zero);
        Assert.True(orderDiscountTotal > Decimal.Zero);
        Assert.True(surchargeTotal > Decimal.Zero);
        Assert.True(finalTotal > Decimal.Zero);

        // Verify the formula: Total = Subtotal - Discounts + Surcharges + Shipping + Taxes
        var expectedTotal = invoice.GetTotalAmount() + invoice.GetTaxAmount();
        Assert.Equal(expectedTotal, finalTotal);
    }

    [Fact]
    public void AddItem_ShouldAddItemToInvoice()
    {
        // Arrange
        var invoice = new Invoice(GuidGenerator.Create(), TestData.Customer(Province), Province, EffectiveDate);
        var item = Product.Create(Guid.NewGuid(), "SKU", "NAME", "STRING", ItemCategory.GeneralGoods)
            .AddPrice(100.00m, EffectiveDate);
        var invoiceItem = new LineItem(Guid.NewGuid())
        {
            InvoiceDate = EffectiveDate,
            SortOrder = 0, // Default sort order, can be adjusted later
            ItemInfo = new ItemInfo
            {
                ItemId = item.Id,
                SKU = item.SKU,
                Name = item.Name,
                Description = item.Description,
                UnitPrice = item.GetUnitPrice(EffectiveDate),
                UnitType = item.UnitType,
                ItemType = item.ItemType,
                ItemCategory = item.ItemCategory,
                TaxCode = item.TaxCode
            },
        };

        // Act
        invoice.AddLineItem(invoiceItem);

        // Assert
        Assert.Single(invoice.LineItems);
        Assert.Equal(100.00m, invoice.GetSubtotal());
    }

    //[Fact]
    //public void LargeInvoice_ShouldCalculateEfficiently()
    //{
    //    // Test with 100+ line items
    //    throw new NotImplementedException();
    //}

    //[Theory]
    //[InlineData(-1, "Amount cannot be negative")]
    //[InlineData(0, "Amount must be greater than zero")]
    //public void Payment_ShouldValidateAmount(Decimal amount, String expectedError)
    //{
    //    // Input validation tests
    //    throw new NotImplementedException();
    //}
}

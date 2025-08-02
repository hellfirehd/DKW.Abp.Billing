namespace Billing.Domain.Tests;

/// <summary>
/// Unit tests for the Invoice domain entity
/// </summary>
public partial class InvoiceTests
{
    private static readonly DateOnly EffectiveDate = new(2025, 01, 01);

    private readonly ITaxProvider _taxProvider = TestData.TaxProvider;
    private readonly ProvinceManager _pm = TestData.ProvinceManager;
    private readonly TestData _testDataBuilder = new();

    [Fact]
    public void SetTaxRates_ShouldApplyCorrectTaxes_ByCategory()
    {
        // Arrange
        var invoice = new Invoice { Province = _pm.GetProvince("BC"), InvoiceDate = EffectiveDate };

        var taxableProduct = TestData.TaxableProductItem(EffectiveDate);
        invoice.AddItem(taxableProduct);

        var taxableService = TestData.TaxableServiceItem(EffectiveDate);
        invoice.AddItem(taxableService);

        var nonTaxableProduct = TestData.NonTaxableProductItem(EffectiveDate);
        invoice.AddItem(nonTaxableProduct);

        // Act
        var allTaxRates = _taxProvider.GetTaxRates(invoice.Province, invoice.InvoiceDate);
        invoice.SetTaxRates(allTaxRates);

        // Assert
        Assert.Equal(24.00m, invoice.GetTotalTax()); // (100 + 100) * 0.12 = 24.00 (5% GST + 7% PST)
        Assert.Equal(2, taxableProduct.AppliedTaxes.Count); // GST + PST
        Assert.Equal(2, taxableService.AppliedTaxes.Count); // GST + PST
        Assert.Empty(nonTaxableProduct.AppliedTaxes); // No taxes
    }

    [Fact]
    public void Invoice_ShouldHandleComplexCalculations()
    {
        // Create a comprehensive test invoice
        var invoice = new Invoice
        {
            Province = _pm.GetProvince("ON"), // 13% HST
            ShippingCost = 15.00m
        };

        // Add multiple items with different tax treatments
        var software = TestData.TaxableProductItem(EffectiveDate);
        invoice.AddItem(software);

        var consulting = TestData.NonTaxableProductItem(EffectiveDate);
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
    public void Invoice_ShouldCalculateCorrectTotalTax_ForMixedItems()
    {
        // Arrange
        var invoice = new Invoice { Province = _pm.GetProvince("BC") }; // BC: 5% GST + 7% PST = 12%

        var taxableProduct = TestData.TaxableProductItem(EffectiveDate);
        invoice.AddItem(taxableProduct);

        var nonTaxableProduct = TestData.NonTaxableProductItem(EffectiveDate);
        invoice.AddItem(nonTaxableProduct);

        var allTaxRates = _taxProvider.GetTaxRates(invoice.Province, invoice.InvoiceDate);

        // Act
        invoice.SetTaxRates(allTaxRates);

        // Assert
        // Only the taxable product should have tax: $100 * 12% = $12
        Assert.Equal(12.00m, invoice.GetTotalTax());
        Assert.Equal(2, taxableProduct.AppliedTaxes.Count); // GST + PST
        Assert.Empty(nonTaxableProduct.AppliedTaxes); // No taxes
        Assert.Equal(162.00m, invoice.GetTotal()); // $150 subtotal + $12 tax
    }

    [Fact]
    public void TaxCalculation_ShouldBeAccurate_ForComplexScenario()
    {
        // Arrange
        var invoice = new Invoice
        {
            Province = _pm.GetProvince("ON"), // 13% HST
            ShippingCost = 10.00m
        };

        var product = TestData.TaxableProductItem(EffectiveDate);

        var service = TestData.TaxableServiceItem(EffectiveDate);

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
        var taxRates = TestData.TaxProvider.GetTaxRates(invoice.Province, invoice.InvoiceDate);
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

    [Fact]
    public void CreateInvoice_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var invoice = new Invoice();

        // Assert
        invoice.Id.ShouldNotBe(Guid.Empty);
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
        Assert.Empty(invoice.Items);
        Assert.Empty(invoice.Payments);
        Assert.Empty(invoice.Refunds);
        Assert.Empty(invoice.OrderDiscounts);
        Assert.Empty(invoice.Surcharges);
        Assert.Equal(0m, invoice.GetSubtotal());
        Assert.Equal(0m, invoice.GetTotal());
    }

    [Fact]
    public void Invoice_AddItem_ShouldAddItemAndUpdateSubtotal()
    {
        // Arrange
        var invoice = new Invoice();
        var product = TestData.TaxableProductItem(EffectiveDate);

        // Act
        invoice.AddItem(product);

        // Assert
        Assert.Single(invoice.Items);
        Assert.Equal(product, invoice.Items[0]);
        Assert.Equal(100.00m, invoice.GetSubtotal());
    }

    [Fact]
    public void Invoice_RemoveItem_ShouldRemoveItemAndUpdateSubtotal()
    {
        // Arrange
        var invoice = new Invoice();
        var product = TestData.TaxableProductItem(EffectiveDate);
        invoice.AddItem(product);

        // Act
        invoice.RemoveItem(product.Id);

        // Assert
        Assert.Empty(invoice.Items);
        Assert.Equal(0m, invoice.GetSubtotal());
    }

    [Fact]
    public void Invoice_SetTaxRates_ShouldApplyTaxes_OnlyOnTaxableItems()
    {
        // Arrange
        var invoice = new Invoice { Province = TestData.NovaScotia };

        var taxableProduct = TestData.TaxableProductItem(EffectiveDate); // $100
        invoice.AddItem(taxableProduct);

        var nonTaxableProduct = TestData.NonTaxableProductItem(EffectiveDate); // $50
        invoice.AddItem(nonTaxableProduct);

        var taxRates = TestData.TaxProvider.GetTaxRates(invoice.Province, invoice.InvoiceDate);

        // Act
        invoice.SetTaxRates(taxRates);

        // Assert
        Assert.Equal(14.00m, invoice.GetTotalTax()); // 100 * 0.14 = 14.00
        Assert.Single(taxableProduct.AppliedTaxes);
        Assert.Empty(nonTaxableProduct.AppliedTaxes);
    }

    [Fact]
    public void Invoice_AddOrderDiscount_ShouldApplyDiscountToTotal()
    {
        // Arrange
        var invoice = new Invoice();
        var product = TestData.TaxableProductItem(EffectiveDate);
        invoice.AddItem(product);

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
        Assert.Single(invoice.OrderDiscounts);
        Assert.Equal(50.00m, invoice.GetOrderDiscounts());
    }

    [Fact]
    public void Invoice_AddSurcharge_ShouldApplySurchargeToTotal()
    {
        // Arrange
        var invoice = new Invoice();
        var product = TestData.TaxableProductItem(EffectiveDate);
        invoice.AddItem(product);

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
        Assert.Equal(3.20m, invoice.GetTotalSurcharges());
    }

    [Fact]
    public void Invoice_AddPayment_ShouldUpdateStatusAndBalance()
    {
        // Arrange
        var invoice = new Invoice();
        var product = TestData.TaxableProductItem(EffectiveDate);
        invoice.AddItem(product);

        var payment = new Payment
        {
            Amount = 100.00m,
            Method = PaymentMethod.CreditCard
        };
        payment.MarkAsCompleted();

        // Act
        invoice.AddPayment(payment);

        // Assert
        Assert.Single(invoice.Payments);
        Assert.Equal(100.00m, invoice.GetTotalPaid());
        Assert.Equal(0m, invoice.GetBalance());
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    [Fact]
    public void Invoice_ProcessRefund_ShouldUpdateStatusAndBalance()
    {
        // Arrange
        var invoice = new Invoice();
        var product = TestData.TaxableProductItem(EffectiveDate);
        invoice.AddItem(product);

        var payment = new Payment
        {
            Amount = 100.00m,
            Method = PaymentMethod.CreditCard
        };
        payment.MarkAsCompleted();
        invoice.AddPayment(payment);

        var refund = new Refund
        {
            Amount = 25.00m,
            Reason = "Partial return"
        };

        // Act
        invoice.ProcessRefund(refund);

        // Assert
        Assert.Single(invoice.Refunds);
        Assert.Equal(25.00m, invoice.GetTotalRefunded());
        Assert.Equal(25.00m, invoice.GetBalance()); // 100 total - 100 paid + 25 refunded = 25
        Assert.Equal(InvoiceStatus.PartiallyRefunded, invoice.Status);
    }

    [Fact]
    public void Invoice_ProcessRefund_ShouldThrowException_WhenRefundExceedsNetPayments()
    {
        // Arrange
        var invoice = new Invoice();
        var product = TestData.TaxableProductItem(EffectiveDate);
        invoice.AddItem(product);

        var payment = new Payment
        {
            Amount = 50.00m, // Only partial payment
            Method = PaymentMethod.CreditCard
        };
        payment.MarkAsCompleted();
        invoice.AddPayment(payment);

        var refund = new Refund
        {
            Amount = 75.00m, // Refund more than paid
            Reason = "Invalid refund"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => invoice.ProcessRefund(refund));
    }

    [Fact]
    public void UpdateStatus_ShouldSetOverdue_WhenPastDueDate()
    {
        // Arrange
        var invoice = new Invoice
        {
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) // Past due
        };
        invoice.AddItem(TestData.TaxableProductItem(EffectiveDate));

        // Act
        invoice.UpdateStatus();

        // Assert
        Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
    }

    [Fact]
    public void MarkAsSent_ShouldUpdateStatus_WhenInDraftOrPending()
    {
        // Arrange
        var invoice = new Invoice(); // Starts as Draft

        // Act
        invoice.MarkAsSent();

        // Assert
        Assert.Equal(InvoiceStatus.Sent, invoice.Status);
    }

    [Fact]
    public void Cancel_Allow_WhenNoPayments()
    {
        // Arrange
        var invoice = new Invoice();
        invoice.AddItem(TestData.TaxableProductItem(EffectiveDate));

        // Act
        invoice.Cancel();

        // Assert
        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenPaymentsExist()
    {
        // Arrange
        var invoice = new Invoice();
        invoice.AddItem(TestData.TaxableProductItem(EffectiveDate));

        var payment = new Payment
        {
            Amount = 50.00m
        };
        payment.MarkAsCompleted();
        invoice.AddPayment(payment);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => invoice.Cancel());
    }

    [Fact]
    public void ComplexScenario_ShouldCalculateCorrectTotals()
    {
        // Arrange - Complex scenario with multiple items, taxes, discounts, and surcharges
        var (invoice, taxRates, discounts) = _testDataBuilder.BuildCompleteTestScenario();

        // Apply order discount for large orders
        var orderDiscount = discounts.First(d => d.Scope == DiscountScope.PerOrder);
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

        // Act
        var subtotal = invoice.GetSubtotal();
        var totalTax = invoice.GetTotalTax();
        var totalDiscounts = invoice.GetLineItemDiscounts() + invoice.GetOrderDiscounts();
        var totalSurcharges = invoice.GetTotalSurcharges();
        var finalTotal = invoice.GetTotal();

        // Assert
        // Verify calculations are reasonable
        Assert.True(subtotal > 0);
        Assert.True(totalTax > 0);
        Assert.True(totalSurcharges > 0);
        Assert.True(finalTotal > 0);

        // Verify the formula: Total = Subtotal - Discounts + Taxes + Surcharges + Shipping
        var expectedTotal = invoice.GetTotalAfterDiscounts() + totalTax + totalSurcharges;
        Assert.Equal(expectedTotal, finalTotal);
    }

    [Fact]
    public void AddItem_ShouldAddItemToInvoice()
    {
        // Arrange
        var invoice = new Invoice();
        var product = TestData.TaxableProductItem(EffectiveDate);

        // Act
        invoice.AddItem(product);

        // Assert
        Assert.Single(invoice.Items);
        Assert.Equal(100.00m, invoice.GetSubtotal());
    }

    [Fact]
    public void SetTaxRates_ShouldApplyTaxesToTaxableItems()
    {
        // Arrange
        var invoice = new Invoice { Province = _pm.GetProvince("BC") };
        var taxableProduct = TestData.TaxableProductItem(EffectiveDate);
        invoice.AddItem(taxableProduct);
        var tax = new Tax("GST", "GST", TaxJurisdiction.Federal).AddTaxRate(0.05m, new DateOnly(2000, 1, 1)); // 5% GST

        // Act
        invoice.SetTaxRates([tax.GetTaxRate(invoice.InvoiceDate)!]);

        // Assert
        Assert.Equal(8.75m, invoice.GetTotalTax());
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

namespace Billing.Domain.Tests;

/// <summary>
/// Unit tests for the Invoice domain entity
/// </summary>
public partial class InvoiceTests
{
    private readonly TestData _testDataBuilder = new();

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
        var product = new Product
        {
            Description = "Test Product",
            UnitPrice = 100.00m,
            Quantity = 2,
            TaxCategory = TaxCategory.TaxableProduct,
            SKU = "TEST-001"
        };

        // Act
        invoice.AddItem(product);

        // Assert
        Assert.Single(invoice.Items);
        Assert.Equal(product, invoice.Items[0]);
        Assert.Equal(200.00m, invoice.GetSubtotal());
    }

    [Fact]
    public void Invoice_RemoveItem_ShouldRemoveItemAndUpdateSubtotal()
    {
        // Arrange
        var invoice = new Invoice();
        var product = new Product
        {
            Description = "Test Product",
            UnitPrice = 100.00m,
            TaxCategory = TaxCategory.TaxableProduct
        };
        invoice.AddItem(product);

        // Act
        invoice.RemoveItem(product.Id);

        // Assert
        Assert.Empty(invoice.Items);
        Assert.Equal(0m, invoice.GetSubtotal());
    }

    [Fact]
    public void Invoice_SetTaxRates_ShouldApplyTaxesToTaxableItems()
    {
        // Arrange
        var taxProvider = new CanadianTaxProvider();
        var invoice = new Invoice { Province = Provinces.NS };
        var taxableProduct = new Product
        {
            Description = "Taxable Product",
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

        var taxRates = taxProvider.GetTaxRates(invoice.Province, invoice.InvoiceDate)
            .ToList();

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
        var product = new Product
        {
            UnitPrice = 500.00m,
            Quantity = 1,
            TaxCategory = TaxCategory.TaxableProduct
        };
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
        var product = new Product
        {
            UnitPrice = 100.00m,
            Quantity = 1,
            TaxCategory = TaxCategory.TaxableProduct
        };
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
        var product = new Product
        {
            UnitPrice = 100.00m,
            Quantity = 1,
            TaxCategory = TaxCategory.TaxableProduct
        };
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
        var product = new Product
        {
            UnitPrice = 100.00m,
            Quantity = 1,
            TaxCategory = TaxCategory.TaxableProduct
        };
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
        var product = new Product
        {
            UnitPrice = 100.00m,
            Quantity = 1,
            TaxCategory = TaxCategory.TaxableProduct
        };
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
        var product = new Product { UnitPrice = 100.00m, Quantity = 1 };
        invoice.AddItem(product);

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
    public void Cancel_ShouldCancelInvoice_WhenNoPayments()
    {
        // Arrange
        var invoice = new Invoice();
        var product = new Product { UnitPrice = 100.00m, Quantity = 1 };
        invoice.AddItem(product);

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
        var product = new Product { UnitPrice = 100.00m, Quantity = 1 };
        invoice.AddItem(product);

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

        // Act & Assert
        var subtotal = invoice.GetSubtotal();
        var totalTax = invoice.GetTotalTax();
        var totalDiscounts = invoice.GetLineItemDiscounts() + invoice.GetOrderDiscounts();
        var totalSurcharges = invoice.GetTotalSurcharges();
        var finalTotal = invoice.GetTotal();

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
        var product = new Product
        {
            Description = "Test Product",
            UnitPrice = 100.00m,
            Quantity = 2,
            TaxCategory = TaxCategory.TaxableProduct
        };

        // Act
        invoice.AddItem(product);

        // Assert
        Assert.Single(invoice.Items);
        Assert.Equal(200.00m, invoice.GetSubtotal());
    }

    [Fact]
    public void SetTaxRates_ShouldApplyTaxesToTaxableItems()
    {
        // Arrange
        var invoice = new Invoice { Province = Provinces.BC };
        var taxableProduct = new Product
        {
            Description = "Taxable Product",
            UnitPrice = 100.00m,
            TaxCategory = TaxCategory.TaxableProduct
        };

        invoice.AddItem(taxableProduct);

        var taxRates = new List<ItemTaxRate>
        {
            new("GST" , TaxCategory.DigitalProduct, new TaxRate(new DateOnly(1991, 1, 1), new DateOnly(2006, 7, 1), 0.0875m))
        };

        // Act
        invoice.SetTaxRates(taxRates);

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
    //public void Payment_ShouldValidateAmount(decimal amount, string expectedError)
    //{
    //    // Input validation tests
    //    throw new NotImplementedException();
    //}
}

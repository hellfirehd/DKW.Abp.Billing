namespace Billing.Application.Tests;

/// <summary>
/// Unit tests for the Invoice Application Service
/// </summary>
public class InvoiceApplicationServiceTests
{
    private readonly ProvinceManager _pm;
    private InvoiceApplicationService SUT { get; }

    public InvoiceApplicationServiceTests()
    {
        SUT = new InvoiceApplicationService(TestData.InvoiceItemManager, TestData.TaxProvider);
        _pm = TestData.ProvinceManager;
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldCreateInvoice_WithValidRequest()
    {
        // Arrange
        var request = new CreateInvoiceRequest
        {
            InvoiceNumber = "INV-TEST-001",
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            State = _pm.GetProvince("NS"),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            Items =
            [
                new CreateInvoiceItemRequest
                {
                    ItemId = TestData.TaxableProductId, // $100.00
                    Quantity = 2.0m
                }
            ],
            ShippingCost = 15.00m,
            ShippingMethod = "Standard"
        };

        // Act
        var invoiceId = await SUT.CreateInvoiceAsync(request);

        // Assert
        invoiceId.ShouldNotBe(Guid.Empty);

        var invoice = await SUT.GetInvoiceAsync(invoiceId);
        Assert.NotNull(invoice);
        Assert.Equal("INV-TEST-001", invoice.InvoiceNumber);
        Assert.Equal("Test Customer", invoice.CustomerName);
        Assert.Single(invoice.Items);
        Assert.Equal(200.00m, invoice.Subtotal);
        Assert.Equal(15.00m, invoice.ShippingCost);
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldApplyTaxes_WhenStateHasTaxRates()
    {
        // Arrange
        var request = new CreateInvoiceRequest
        {
            InvoiceNumber = "INV-TAX-001",
            CustomerName = "Tax Test Customer",
            State = _pm.GetProvince("NS"),
            Items =
            [
                new CreateInvoiceItemRequest
                {
                    ItemId = TestData.TaxableProductId,
                    Quantity = 1,
                }
            ]
        };

        // Act
        var invoiceId = await SUT.CreateInvoiceAsync(request);

        // Assert
        var invoice = await SUT.GetInvoiceAsync(invoiceId);
        Assert.NotNull(invoice);
        Assert.True(invoice.TotalTax > 0); // Should have tax applied
        Assert.Equal(14.00m, invoice.TotalTax); // 100 * 0.14 = 14.00
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldCreateService_WhenItemTypeIsService()
    {
        // Arrange
        var request = new CreateInvoiceRequest
        {
            InvoiceNumber = "INV-SERVICE-001",
            CustomerName = "Service Customer",
            State = _pm.GetProvince("NS"),
            Items =
            [
                new CreateInvoiceItemRequest
                {
                    ItemId = TestData.TaxableServiceId, // $500.00
                    Quantity = 4.0m
                }
            ]
        };

        // Act
        var invoiceId = await SUT.CreateInvoiceAsync(request);

        // Assert
        var invoice = await SUT.GetInvoiceAsync(invoiceId);
        Assert.NotNull(invoice);
        Assert.Single(invoice.Items);
        Assert.Equal(2000.00m, invoice.Subtotal); // 500 * 4 = 2000
        Assert.Equal(ItemType.Service, invoice.Items[0].ItemType);
    }

    [Fact]
    public async Task GetInvoiceAsync_ShouldReturnNull_WhenInvoiceNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await SUT.GetInvoiceAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddInvoiceItemAsync_ShouldAddItem_ToExistingInvoice()
    {
        // Arrange
        var invoiceId = await CreateTestInvoice();
        var itemRequest = new CreateInvoiceItemRequest
        {
            ItemId = TestData.TaxableServiceId,
            Quantity = 1,
        };

        // Act
        await SUT.AddInvoiceItemAsync(invoiceId, itemRequest);

        // Assert
        var invoice = await SUT.GetInvoiceAsync(invoiceId);
        Assert.NotNull(invoice);
        Assert.Equal(2, invoice.Items.Count);
        Assert.Equal(600.00m, invoice.Subtotal); // Original 100 + new 500
    }

    [Fact]
    public async Task AddInvoiceItemAsync_ShouldThrowException_WhenInvoiceNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var itemRequest = new CreateInvoiceItemRequest
        {
            ItemId = TestData.TaxableProductId,
            Quantity = 1,
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            SUT.AddInvoiceItemAsync(nonExistentId, itemRequest));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldAddPayment_AndUpdateInvoiceStatus()
    {
        // Arrange
        var invoiceId = await CreateTestInvoice();
        var invoice = await SUT.GetInvoiceAsync(invoiceId);

        var paymentRequest = new CreatePaymentRequest
        {
            InvoiceId = invoiceId,
            Amount = invoice!.Total, // Full payment
            Method = PaymentMethod.CreditCard,
            ReferenceNumber = "CC-001",
            Notes = "Test payment",
            GatewayTransactionId = "TXN-12345"
        };

        // Act
        var paymentId = await SUT.ProcessPaymentAsync(paymentRequest);

        // Assert
        paymentId.ShouldNotBe(Guid.Empty);

        var updatedInvoice = await SUT.GetInvoiceAsync(invoiceId);
        Assert.NotNull(updatedInvoice);
        Assert.Single(updatedInvoice.Payments);
        Assert.Equal(PaymentStatus.Completed, updatedInvoice.Payments[0].Status);
        Assert.Equal(0m, updatedInvoice.Balance);
        Assert.Equal(InvoiceStatus.Paid, updatedInvoice.Status);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowException_WhenInvoiceNotFound()
    {
        // Arrange
        var paymentRequest = new CreatePaymentRequest
        {
            InvoiceId = Guid.NewGuid(),
            Amount = 100.00m,
            Method = PaymentMethod.CreditCard
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            SUT.ProcessPaymentAsync(paymentRequest));
    }

    [Fact]
    public async Task ProcessRefundAsync_ShouldCreateRefund_WithProportionalBreakdown()
    {
        // Arrange
        var invoiceId = await CreateTestInvoiceWithPayment();
        var invoice = await SUT.GetInvoiceAsync(invoiceId);

        var refundRequest = new CreateRefundRequest
        {
            InvoiceId = invoiceId,
            Amount = invoice!.Total * 0.5m, // 50% refund
            Reason = "Product defect",
            ReferenceNumber = "REF-001"
        };

        // Act
        var refundId = await SUT.ProcessRefundAsync(refundRequest);

        // Assert
        refundId.ShouldNotBe(Guid.Empty);

        var updatedInvoice = await SUT.GetInvoiceAsync(invoiceId);
        Assert.NotNull(updatedInvoice);
        Assert.Single(updatedInvoice.Refunds);
        Assert.Equal(InvoiceStatus.PartiallyRefunded, updatedInvoice.Status);
        Assert.True(updatedInvoice.Balance > 0);
    }

    [Fact]
    public async Task ProcessRefundAsync_ShouldThrowException_WhenRefundExceedsPayments()
    {
        // Arrange
        var invoiceId = await CreateTestInvoice(); // No payments
        var invoice = await SUT.GetInvoiceAsync(invoiceId);

        var refundRequest = new CreateRefundRequest
        {
            InvoiceId = invoiceId,
            Amount = 100.00m, // More than paid (which is 0)
            Reason = "Invalid refund"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            SUT.ProcessRefundAsync(refundRequest));
    }

    [Fact]
    public async Task CompleteWorkflow_ShouldHandleFullInvoiceLifecycle()
    {
        // Arrange & Act - Create invoice
        var invoiceId = await CreateTestInvoice();
        var invoice = await SUT.GetInvoiceAsync(invoiceId);
        Assert.NotNull(invoice);
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);

        // Add additional item
        await SUT.AddInvoiceItemAsync(invoiceId, new CreateInvoiceItemRequest
        {
            ItemId = TestData.TaxableServiceId,
            Quantity = 2.0m
        });

        // Make partial payment
        var partialPaymentId = await SUT.ProcessPaymentAsync(new CreatePaymentRequest
        {
            InvoiceId = invoiceId,
            Amount = 150.00m, // Partial payment
            Method = PaymentMethod.BankTransfer,
            ReferenceNumber = "BT-001"
        });

        // Make remaining payment
        invoice = await SUT.GetInvoiceAsync(invoiceId);
        var remainingBalance = invoice!.Balance;

        var finalPaymentId = await SUT.ProcessPaymentAsync(new CreatePaymentRequest
        {
            InvoiceId = invoiceId,
            Amount = remainingBalance,
            Method = PaymentMethod.CreditCard,
            ReferenceNumber = "CC-002"
        });

        // Process partial refund
        var refundId = await SUT.ProcessRefundAsync(new CreateRefundRequest
        {
            InvoiceId = invoiceId,
            Amount = 50.00m,
            Reason = "Customer satisfaction",
            ReferenceNumber = "REF-001"
        });

        // Assert final state
        var finalInvoice = await SUT.GetInvoiceAsync(invoiceId);
        Assert.NotNull(finalInvoice);
        Assert.Equal(2, finalInvoice.Items.Count);
        Assert.Equal(2, finalInvoice.Payments.Count);
        Assert.Single(finalInvoice.Refunds);
        Assert.Equal(InvoiceStatus.PartiallyRefunded, finalInvoice.Status);
        Assert.True(finalInvoice.TotalPaid > finalInvoice.TotalRefunded);
        Assert.Equal(50.00m, finalInvoice.Balance); // Should equal refund amount
    }

    /// <summary>
    /// Creates a test invoice with a single taxable item with a fixed price of $100.00.
    /// </summary>
    /// <returns></returns>
    private async Task<Guid> CreateTestInvoice()
    {
        var request = new CreateInvoiceRequest
        {
            InvoiceNumber = $"INV-TEST-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            State = _pm.GetProvince("NS"),
            Items =
            [
                new CreateInvoiceItemRequest
                {
                    ItemId = TestData.TaxableProductId,
                    Quantity = 1,
                }
            ]
        };

        return await SUT.CreateInvoiceAsync(request);
    }

    private async Task<Guid> CreateTestInvoiceWithPayment()
    {
        var invoiceId = await CreateTestInvoice();
        var invoice = await SUT.GetInvoiceAsync(invoiceId);

        await SUT.ProcessPaymentAsync(new CreatePaymentRequest
        {
            InvoiceId = invoiceId,
            Amount = invoice!.Total,
            Method = PaymentMethod.CreditCard,
            ReferenceNumber = "CC-SETUP"
        });

        return invoiceId;
    }
}

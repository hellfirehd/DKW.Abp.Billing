using Billing.Invoices;
using Billing.Payments;
using Billing.Refunds;
using Shouldly;

namespace Billing.Application.Tests;

/// <summary>
/// Unit tests for the Invoice Application Service
/// </summary>
public class InvoiceApplicationServiceTests
{
    private readonly InvoiceApplicationService _invoiceService;
    private readonly BillingTestDataBuilder _testDataBuilder;

    public InvoiceApplicationServiceTests()
    {
        _invoiceService = new InvoiceApplicationService();
        _testDataBuilder = new BillingTestDataBuilder();
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
            State = Provinces.NS,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            Items =
            [
                new CreateInvoiceItemRequest
                {
                    Description = "Test Product",
                    UnitPrice = 100.00m,
                    Quantity = 2,
                    Category = TaxCategory.TaxableProduct,
                    ItemType = "Product",
                    SKU = "TEST-001"
                }
            ],
            ShippingCost = 15.00m,
            ShippingMethod = "Standard"
        };

        // Act
        var invoiceId = await _invoiceService.CreateInvoiceAsync(request);

        // Assert
        invoiceId.ShouldNotBe(Guid.Empty);

        var invoice = await _invoiceService.GetInvoiceAsync(invoiceId);
        Assert.NotNull(invoice);
        Assert.Equal("INV-TEST-001", invoice.InvoiceNumber);
        Assert.Equal("Test Customer", invoice.CustomerName);
        Assert.Single(invoice.Items);
        Assert.Equal(200.00m, invoice.Subtotal);
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldApplyTaxes_WhenStateHasTaxRates()
    {
        // Arrange
        var request = new CreateInvoiceRequest
        {
            InvoiceNumber = "INV-TAX-001",
            CustomerName = "Tax Test Customer",
            State = Provinces.NS,
            Items =
            [
                new CreateInvoiceItemRequest
                {
                    Description = "Taxable Product",
                    UnitPrice = 100.00m,
                    Quantity = 1,
                    Category = TaxCategory.TaxableProduct,
                    ItemType = "Product"
                }
            ]
        };

        // Act
        var invoiceId = await _invoiceService.CreateInvoiceAsync(request);

        // Assert
        var invoice = await _invoiceService.GetInvoiceAsync(invoiceId);
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
            State = Provinces.NS,
            Items =
            [
                new CreateInvoiceItemRequest
                {
                    Description = "Consulting Service",
                    ItemType = "Service",
                    Category = TaxCategory.TaxableService,
                    HourlyRate = 150.00m,
                    Hours = 4.0m
                }
            ]
        };

        // Act
        var invoiceId = await _invoiceService.CreateInvoiceAsync(request);

        // Assert
        var invoice = await _invoiceService.GetInvoiceAsync(invoiceId);
        Assert.NotNull(invoice);
        Assert.Single(invoice.Items);
        Assert.Equal(600.00m, invoice.Subtotal); // 150 * 4 = 600
        Assert.Equal("Service", invoice.Items[0].ItemType);
    }

    [Fact]
    public async Task GetInvoiceAsync_ShouldReturnNull_WhenInvoiceNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _invoiceService.GetInvoiceAsync(nonExistentId);

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
            Description = "Additional Product",
            UnitPrice = 50.00m,
            Quantity = 1,
            Category = TaxCategory.TaxableProduct,
            ItemType = "Product"
        };

        // Act
        await _invoiceService.AddInvoiceItemAsync(invoiceId, itemRequest);

        // Assert
        var invoice = await _invoiceService.GetInvoiceAsync(invoiceId);
        Assert.NotNull(invoice);
        Assert.Equal(2, invoice.Items.Count);
        Assert.Equal(150.00m, invoice.Subtotal); // Original 100 + new 50
    }

    [Fact]
    public async Task AddInvoiceItemAsync_ShouldThrowException_WhenInvoiceNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var itemRequest = new CreateInvoiceItemRequest
        {
            Description = "Test Product",
            UnitPrice = 100.00m,
            Category = TaxCategory.TaxableProduct
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _invoiceService.AddInvoiceItemAsync(nonExistentId, itemRequest));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldAddPayment_AndUpdateInvoiceStatus()
    {
        // Arrange
        var invoiceId = await CreateTestInvoice();
        var invoice = await _invoiceService.GetInvoiceAsync(invoiceId);

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
        var paymentId = await _invoiceService.ProcessPaymentAsync(paymentRequest);

        // Assert
        paymentId.ShouldNotBe(Guid.Empty);

        var updatedInvoice = await _invoiceService.GetInvoiceAsync(invoiceId);
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
            _invoiceService.ProcessPaymentAsync(paymentRequest));
    }

    [Fact]
    public async Task ProcessRefundAsync_ShouldCreateRefund_WithProportionalBreakdown()
    {
        // Arrange
        var invoiceId = await CreateTestInvoiceWithPayment();
        var invoice = await _invoiceService.GetInvoiceAsync(invoiceId);

        var refundRequest = new CreateRefundRequest
        {
            InvoiceId = invoiceId,
            Amount = invoice!.Total * 0.5m, // 50% refund
            Reason = "Product defect",
            ReferenceNumber = "REF-001"
        };

        // Act
        var refundId = await _invoiceService.ProcessRefundAsync(refundRequest);

        // Assert
        refundId.ShouldNotBe(Guid.Empty); ;

        var updatedInvoice = await _invoiceService.GetInvoiceAsync(invoiceId);
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
        var invoice = await _invoiceService.GetInvoiceAsync(invoiceId);

        var refundRequest = new CreateRefundRequest
        {
            InvoiceId = invoiceId,
            Amount = 100.00m, // More than paid (which is 0)
            Reason = "Invalid refund"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _invoiceService.ProcessRefundAsync(refundRequest));
    }

    [Fact]
    public async Task CompleteWorkflow_ShouldHandleFullInvoiceLifecycle()
    {
        // Arrange & Act - Create invoice
        var invoiceId = await CreateTestInvoice();
        var invoice = await _invoiceService.GetInvoiceAsync(invoiceId);
        Assert.NotNull(invoice);
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);

        // Add additional item
        await _invoiceService.AddInvoiceItemAsync(invoiceId, new CreateInvoiceItemRequest
        {
            Description = "Additional Service",
            ItemType = "Service",
            Category = TaxCategory.TaxableService,
            HourlyRate = 100.00m,
            Hours = 2.0m
        });

        // Make partial payment
        var partialPaymentId = await _invoiceService.ProcessPaymentAsync(new CreatePaymentRequest
        {
            InvoiceId = invoiceId,
            Amount = 150.00m, // Partial payment
            Method = PaymentMethod.BankTransfer,
            ReferenceNumber = "BT-001"
        });

        // Make remaining payment
        invoice = await _invoiceService.GetInvoiceAsync(invoiceId);
        var remainingBalance = invoice!.Balance;

        var finalPaymentId = await _invoiceService.ProcessPaymentAsync(new CreatePaymentRequest
        {
            InvoiceId = invoiceId,
            Amount = remainingBalance,
            Method = PaymentMethod.CreditCard,
            ReferenceNumber = "CC-002"
        });

        // Process partial refund
        var refundId = await _invoiceService.ProcessRefundAsync(new CreateRefundRequest
        {
            InvoiceId = invoiceId,
            Amount = 50.00m,
            Reason = "Customer satisfaction",
            ReferenceNumber = "REF-001"
        });

        // Assert final state
        var finalInvoice = await _invoiceService.GetInvoiceAsync(invoiceId);
        Assert.NotNull(finalInvoice);
        Assert.Equal(2, finalInvoice.Items.Count);
        Assert.Equal(2, finalInvoice.Payments.Count);
        Assert.Single(finalInvoice.Refunds);
        Assert.Equal(InvoiceStatus.PartiallyRefunded, finalInvoice.Status);
        Assert.True(finalInvoice.TotalPaid > finalInvoice.TotalRefunded);
        Assert.Equal(50.00m, finalInvoice.Balance); // Should equal refund amount
    }

    private async Task<Guid> CreateTestInvoice()
    {
        var request = new CreateInvoiceRequest
        {
            InvoiceNumber = $"INV-TEST-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            State = Provinces.NS,
            Items =
            [
                new CreateInvoiceItemRequest
                {
                    Description = "Test Product",
                    UnitPrice = 100.00m,
                    Quantity = 1,
                    Category = TaxCategory.TaxableProduct,
                    ItemType = "Product"
                }
            ]
        };

        return await _invoiceService.CreateInvoiceAsync(request);
    }

    private async Task<Guid> CreateTestInvoiceWithPayment()
    {
        var invoiceId = await CreateTestInvoice();
        var invoice = await _invoiceService.GetInvoiceAsync(invoiceId);

        await _invoiceService.ProcessPaymentAsync(new CreatePaymentRequest
        {
            InvoiceId = invoiceId,
            Amount = invoice!.Total,
            Method = PaymentMethod.CreditCard,
            ReferenceNumber = "CC-SETUP"
        });

        return invoiceId;
    }
}

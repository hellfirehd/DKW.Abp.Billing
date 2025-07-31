using System;

namespace Billing;

//public abstract class BillingTestBase<TStartupModule> : AbpIntegratedTest<TStartupModule>
//    where TStartupModule : IAbpModule
//{
//    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
//    {
//        options.UseAutofac();
//    }
//}

/// <summary>
/// Base class for billing system unit tests
/// </summary>
public abstract class BillingTestBase
{
    protected readonly BillingTestDataBuilder TestDataBuilder;

    protected BillingTestBase()
    {
        TestDataBuilder = new BillingTestDataBuilder();
    }

    /// <summary>
    /// Creates a sample product for testing
    /// </summary>
    protected Product CreateTestProduct(
        string description = "Test Product",
        decimal unitPrice = 100.00m,
        int quantity = 1,
        TaxCategory category = TaxCategory.TaxableProduct,
        string sku = "TEST-001")
    {
        return new Product
        {
            Description = description,
            UnitPrice = unitPrice,
            Quantity = quantity,
            TaxCategory = category,
            SKU = sku,
            Weight = 1.0m,
            Manufacturer = "Test Manufacturer",
            RequiresShipping = true
        };
    }

    /// <summary>
    /// Creates a sample service for testing
    /// </summary>
    protected Service CreateTestService(
        string description = "Test Service",
        decimal hourlyRate = 150.00m,
        decimal hours = 2.0m,
        TaxCategory category = TaxCategory.TaxableService)
    {
        return new Service
        {
            Description = description,
            HourlyRate = hourlyRate,
            Hours = hours,
            TaxCategory = category
        };
    }

    /// <summary>
    /// Creates a sample tax rate for testing
    /// </summary>
    protected TaxRate CreateTestTaxRate(
        string state = "CA",
        TaxCategory category = TaxCategory.TaxableProduct,
        decimal rate = 0.0875m, // 8.75%
        string description = "California Sales Tax")
    {
        return new TaxRate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)), null, rate)
        {
            TaxCategory = category,
            Name = description,
            IsActive = true
        };
    }

    /// <summary>
    /// Creates a sample discount for testing
    /// </summary>
    protected Discount CreateTestDiscount(
        string name = "Test Discount",
        DiscountType type = DiscountType.Percentage,
        DiscountScope scope = DiscountScope.PerItem,
        decimal value = 0.10m) // 10%
    {
        return new Discount
        {
            Name = name,
            Description = $"{name} Description",
            Type = type,
            Scope = scope,
            Value = value,
            IsActive = true,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30))
        };
    }

    /// <summary>
    /// Creates a sample surcharge for testing
    /// </summary>
    protected Surcharge CreateTestSurcharge(
        string name = "Credit Card Fee",
        SurchargeType type = SurchargeType.CreditCardProcessing,
        decimal fixedAmount = 0.30m,
        decimal percentageRate = 0.029m) // 2.9%
    {
        return new Surcharge
        {
            Name = name,
            Description = $"{name} Description",
            Type = type,
            FixedAmount = fixedAmount,
            PercentageRate = percentageRate,
            IsActive = true
        };
    }

    /// <summary>
    /// Creates a sample invoice for testing
    /// </summary>
    protected Invoice CreateTestInvoice(
        Province state,
        string customerName = "Test Customer")
    {
        return new Invoice
        {
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-001",
            CustomerName = customerName,
            CustomerEmail = "test@example.com",
            BillingAddress = "123 Test St, Test City, CA 90210",
            ShippingAddress = "123 Test St, Test City, CA 90210",
            Province = state,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            ShippingCost = 15.00m,
            ShippingMethod = "Standard Shipping"
        };
    }

    /// <summary>
    /// Creates a sample payment for testing
    /// </summary>
    protected Payment CreateTestPayment(
        IPaymentMethod method,
        decimal amount = 100.00m,
        PaymentStatus status = PaymentStatus.Completed)
    {
        var payment = new Payment
        {
            Amount = amount,
            Method = method,
            ReferenceNumber = $"PAY-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            Notes = "Test payment",
            GatewayTransactionId = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            GatewayName = "TestGateway"
        };

        if (status == PaymentStatus.Completed)
        {
            payment.MarkAsCompleted();
        }

        return payment;
    }
}

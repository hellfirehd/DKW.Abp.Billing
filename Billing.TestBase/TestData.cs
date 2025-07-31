using Bogus;
using Volo.Abp.DependencyInjection;

namespace Billing;

/// <summary>
/// Builder class for creating test data for billing system tests
/// </summary>
public class TestData : ITransientDependency
{
    /// <summary>
    /// Creates a complete test scenario with invoice, items, taxes, and payments
    /// </summary>
    public (Invoice invoice, List<ItemTaxRate> taxRates, List<Discount> discounts) BuildCompleteTestScenario()
    {
        // Create tax rates for Nova Scotia
        var taxRates = new List<ItemTaxRate>
        {
            new("BC-PST", TaxCategory.TaxableService, new TaxRate(new DateOnly(2010, 7, 1), DateOnly.MaxValue, 0.0875m)),
            new("GST", TaxCategory.TaxableProduct, new TaxRate(new DateOnly(2010, 7, 1), DateOnly.MaxValue,0.0875m))
        };

        // Create discounts
        var discounts = new List<Discount>
        {
            new() {
                Name = "10% Item Discount",
                Type = DiscountType.Percentage,
                Scope = DiscountScope.PerItem,
                Value = 0.10m, // 10%
                IsActive = true
            },
            new() {
                Name = "$50 Order Discount",
                Type = DiscountType.FixedAmount,
                Scope = DiscountScope.PerOrder,
                Value = 50.00m,
                MinimumAmount = 500.00m, // Only applies to orders over $500
                IsActive = true
            }
        };

        // Create invoice with items
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-2024-001",
            Customer = Customer(),
            Province = Provinces.NS,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            ShippingCost = 25.00m,
            ShippingMethod = "Express Shipping"
        };

        // Add a taxable product
        var product = new Product
        {
            Description = "Professional Software License",
            UnitPrice = 299.99m,
            Quantity = 3,
            TaxCategory = TaxCategory.TaxableProduct,
            SKU = "PSL-001",
            Weight = 0m, // Digital product
            Manufacturer = "Software Corp",
            RequiresShipping = false
        };

        // Add a taxable service
        var service = new Service
        {
            Description = "Implementation Consulting",
            HourlyRate = 200.00m,
            Hours = 8.0m,
            TaxCategory = TaxCategory.TaxableService
        };

        // Add a non-taxable item
        var nonTaxableProduct = new Product
        {
            Description = "Educational Material",
            UnitPrice = 29.99m,
            Quantity = 2,
            TaxCategory = TaxCategory.NonTaxableProduct,
            SKU = "EDU-001",
            Weight = 0.5m,
            Manufacturer = "Education Inc",
            RequiresShipping = true
        };

        invoice.AddItem(product);
        invoice.AddItem(service);
        invoice.AddItem(nonTaxableProduct);

        // Apply tax rates
        invoice.SetTaxRates(taxRates);

        return (invoice, taxRates, discounts);
    }

    /// <summary>
    /// Creates a simple invoice for basic testing
    /// </summary>
    public Invoice BuildSimpleInvoice()
    {
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-SIMPLE-001",
            Customer = Customer(Provinces.BC),
            Province = Provinces.BC,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
        };

        var product = new Product
        {
            Description = "Test Product",
            UnitPrice = 100.00m,
            Quantity = 1,
            TaxCategory = TaxCategory.TaxableProduct,
            SKU = "TEST-001"
        };

        invoice.AddItem(product);

        return invoice;
    }

    public static Customer Customer(Province? province = null) => new Faker<Customer>()
        .RuleFor(c => c.Id, f => f.Random.Guid())
        .RuleFor(c => c.Name, f => f.Person.FullName)
        .RuleFor(c => c.Addresses, f => [Address(isDefault: false, isBilling: true), Address(isDefault: false, isShipping: true)])
        .RuleFor(c => c.PhoneNumbers, _ => [])
        .RuleFor(c => c.EmailAddresses, f => [new EmailAddress(f.Person.Email, f.Person.FullName)])
        .RuleFor(c => c.PaymentMethods, f => [])
        .RuleFor(c => c.TaxProfile, f => default);

    public static Address Address(Province? province = null, Boolean isDefault = false, Boolean? isBilling = null, Boolean? isShipping = null) => new Faker<Address>()
        .RuleFor(a => a.Id, f => f.Random.Guid())
        .RuleFor(a => a.Line1, f => f.Address.StreetAddress())
        .RuleFor(a => a.Line2, f => f.Lorem.Word())
        .RuleFor(a => a.City, f => f.Address.City())
        .RuleFor(a => a.Province, f => province is null ? f.PickRandom(Provinces.All) : province)
        .RuleFor(a => a.PostalCode, f => f.Address.ZipCode())
        .RuleFor(a => a.Country, f => "Canada")
        .RuleFor(a => a.IsDefault, f => isDefault)
        .RuleFor(a => a.IsShippingAddress, f => isDefault ? true : isShipping.HasValue ? isShipping : f.Random.Bool())
        .RuleFor(a => a.IsBillingAddress, f => isDefault ? true : isBilling.HasValue ? isBilling : f.Random.Bool());

    public static PhoneNumber PhoneNumber() => new Faker<PhoneNumber>()
        .RuleFor(p => p.CountryCode, 1)
        .RuleFor(p => p.Number, f => f.Random.Int(111111111, 99999999));

    /// <summary>
    /// Creates payment and refund test scenarios
    /// </summary>
    public (Payment fullPayment, Payment partialPayment, Refund refund) BuildPaymentScenarios(decimal invoiceTotal)
    {
        var fullPayment = new Payment
        {
            Amount = invoiceTotal,
            Method = PaymentMethod.CreditCard,
            ReferenceNumber = "CC-FULL-001",
            Notes = "Full payment via credit card",
            GatewayTransactionId = "TXN-12345",
            GatewayName = "Stripe"
        };
        fullPayment.MarkAsCompleted();

        var partialPayment = new Payment
        {
            Amount = invoiceTotal * 0.5m, // 50% payment
            Method = PaymentMethod.BankTransfer,
            ReferenceNumber = "BT-PARTIAL-001",
            Notes = "Partial payment via bank transfer"
        };
        partialPayment.MarkAsCompleted();

        var refund = new Refund
        {
            Amount = invoiceTotal * 0.25m, // 25% refund
            Reason = "Product returned",
            ReferenceNumber = "REF-001",
            Notes = "Customer returned one item"
        };

        return (fullPayment, partialPayment, refund);
    }
}

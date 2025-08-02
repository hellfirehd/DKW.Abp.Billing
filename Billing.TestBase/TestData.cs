using Billing.Customers;
using Billing.EntityFrameworkCore;
using Bogus;

namespace Billing;

/// <summary>
/// Builder class for creating test data for billing system tests
/// </summary>
public class TestData
{
    public static readonly DateOnly EffectiveDate = new(1970, 01, 01);

    /// <summary>
    /// Represents the unique identifier for a taxable product worth $100.00.
    /// </summary>
    public static readonly Guid TaxableProductId = Guid.NewGuid();

    /// <summary>
    /// Represents the unique identifier for a non-taxable <see cref="ItemCategory.BasicGroceries"/> product worth $50.00.
    /// </summary>
    public static readonly Guid NonTaxableProductId = Guid.NewGuid();

    /// <summary>
    /// Represents the unique identifier for a taxable service worth $500.00.
    /// </summary>
    public static readonly Guid TaxableServiceId = Guid.NewGuid();

    /// <summary>
    /// Represents the unique identifier for a taxable service worth $75.00.
    /// </summary>
    public static readonly Guid NonTaxableServiceId = Guid.NewGuid();

    static TestData()
    {
        ProvinceManager = new ProvinceManager(new FakeProvinceRepository());
        TaxCodeRepository = new FakeTaxCodeRepository();
        TaxProvider = new CanadianTaxProvider(ProvinceManager, TimeProvider.System);
        ItemRepository = new FakeItemRepository();
        InvoiceItemManager = new InvoiceItemManager(ItemRepository, [new ProductInvoiceItemFactory(), new ServiceInvoiceItemFactory()]);
        InvoiceManager = new InvoiceManager(ItemRepository, TaxCodeRepository, TaxProvider, ProvinceManager, InvoiceItemManager);

        ItemRepository.AddItem(TaxableProduct(TestData.TaxableProductId));
        ItemRepository.AddItem(NonTaxableProduct(TestData.NonTaxableProductId));
        ItemRepository.AddItem(TaxableService(TestData.TaxableServiceId));
        ItemRepository.AddItem(NonTaxableService(TestData.NonTaxableServiceId));
    }

    public static ITaxCodeRepository TaxCodeRepository { get; }
    public static ITaxProvider TaxProvider { get; }
    public static IItemRepository ItemRepository { get; }
    public static ProvinceManager ProvinceManager { get; }
    public static InvoiceManager InvoiceManager { get; }
    public static InvoiceItemManager InvoiceItemManager { get; }
    public static Province NovaScotia => ProvinceManager.GetProvince("NS");
    public static Province BritishColumbia => ProvinceManager.GetProvince("BC");
    public static Province Alberta => ProvinceManager.GetProvince("AB");
    public static Province Ontario => ProvinceManager.GetProvince("ON");

    /// <summary>
    /// Creates a complete test scenario with invoice, items, taxes, and payments
    /// </summary>
    public (Invoice invoice, IEnumerable<TaxRate> taxRates, IEnumerable<Discount> discounts) BuildCompleteTestScenario()
    {
        // Create tax rates for Nova Scotia
        var taxRates = TaxProvider.GetTaxRates(ProvinceManager.GetProvince("NS"));

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
            Province = ProvinceManager.GetProvince("NS"),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            ShippingCost = 25.00m,
            ShippingMethod = "Express Shipping"
        };

        invoice.AddItem(CreateInvoiceItem(TestData.TaxableProductId, invoice.InvoiceDate, 1));
        invoice.AddItem(CreateInvoiceItem(TestData.TaxableServiceId, invoice.InvoiceDate, 1));
        invoice.AddItem(CreateInvoiceItem(TestData.NonTaxableProductId, invoice.InvoiceDate, 1));

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
            Customer = Customer(ProvinceManager.GetProvince("BC")),
            Province = ProvinceManager.GetProvince("BC"),
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
        };

        invoice.AddItem(CreateInvoiceItem(TestData.TaxableProductId, invoice.InvoiceDate));

        return invoice;
    }

    public static Customer Customer(Province? province = null) => new Faker<Customer>()
        .RuleFor(c => c.Id, f => f.Random.Guid())
        .RuleFor(c => c.Name, f => f.Person.FullName)
        .RuleFor(c => c.Addresses, f => [Address(isDefault: false, isBilling: true), Address(isDefault: false, isShipping: true)])
        .RuleFor(c => c.EmailAddresses, f => new Email(f.Person.Email))
        .RuleFor(c => c.PaymentMethods, f => [])
        .RuleFor(c => c.TaxProfile, f => default);

    public static Address Address(Province? province = null, Boolean isDefault = false, Boolean? isBilling = null, Boolean? isShipping = null) => new Faker<Address>()
        .RuleFor(a => a.Line1, f => f.Address.StreetAddress())
        .RuleFor(a => a.Line2, f => f.Lorem.Word())
        .RuleFor(a => a.City, f => f.Address.City())
        .RuleFor(a => a.Province, f => province is null ? f.PickRandom(ProvinceManager.GetAllProvinces()) : province)
        .RuleFor(a => a.PostalCode, f => f.Address.ZipCode())
        .RuleFor(a => a.Country, f => "Canada")
        .RuleFor(a => a.IsDefault, f => isDefault)
        .RuleFor(a => a.IsShippingAddress, f => isDefault ? true : isShipping.HasValue ? isShipping : f.Random.Bool())
        .RuleFor(a => a.IsBillingAddress, f => isDefault ? true : isBilling.HasValue ? isBilling : f.Random.Bool())
        .RuleFor(a => a.Name, f => f.Person.FullName)
        .RuleFor(a => a.PhoneNumber, f => new PhoneNumber(f.Person.Phone));

    /// <summary>
    /// Creates a taxable <see cref="ItemCategory.BooksAndMagazines"/> product worth $50.00.
    /// </summary>
    public static Product TaxableProduct(Guid id) => Product.Create(id, "BOOK-001", "Test Product", ItemCategory.BooksAndMagazines).AddPrice(100.00m, EffectiveDate);

    /// <summary>
    /// Creates a non-taxable <see cref="ItemCategory.BasicGroceries"/> product worth $50.00.
    /// </summary>
    public static Product NonTaxableProduct(Guid id) => Product.Create(id, "BEEF-002", "Non-Taxable Product", ItemCategory.BasicGroceries).AddPrice(50.00m, EffectiveDate);

    /// <summary>
    /// Creates a taxable <see cref="ItemCategory.ProfessionalServices"/> service worth $500.00.
    /// </summary>
    public static Service TaxableService(Guid id) => Service.Create(id, "Consulting Service", "Provider A", ItemCategory.ProfessionalServices).AddPrice(500.00m, EffectiveDate);

    /// <summary>
    /// Creates a non-taxable <see cref="ItemCategory.EducationalServices"/> service worth $75.00.
    /// </summary>
    public static Service NonTaxableService(Guid id) => Service.Create(id, "Financial Advice", "Provider B", ItemCategory.EducationalServices).AddPrice(75.00m, EffectiveDate);

    public static InvoiceItem CreateInvoiceItem(Guid itemId, DateOnly effectiveDate, Decimal quantity = 1.0m)
    {
        return InvoiceItemManager.Create(itemId, effectiveDate, quantity);
    }

    public static InvoiceItem TaxableProductItem(DateOnly effectiveDate)
    {
        return CreateInvoiceItem(TestData.TaxableProductId, effectiveDate);
    }

    public static InvoiceItem NonTaxableProductItem(DateOnly effectiveDate)
    {
        return CreateInvoiceItem(TestData.NonTaxableProductId, effectiveDate);
    }
    public static InvoiceItem TaxableServiceItem(DateOnly effectiveDate)
    {
        return CreateInvoiceItem(TestData.TaxableServiceId, effectiveDate);
    }
    public static InvoiceItem NonTaxableServiceItem(DateOnly effectiveDate)
    {
        return CreateInvoiceItem(TestData.NonTaxableServiceId, effectiveDate);
    }
}

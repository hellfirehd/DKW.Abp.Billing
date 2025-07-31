namespace Billing.Examples;

/// <summary>
/// Demonstrates how to use the Canadian tax code system
/// </summary>
public static class CanadianTaxCodeExamples
{
    /// <summary>
    /// Example: Creating tax codes for different Canadian tax treatments
    /// </summary>
    public static void CreateCanadianTaxCodes()
    {
        Console.WriteLine("=== Canadian Tax Code System Examples ===\n");

        // Standard taxable goods
        var standardGoods = new TaxCode
        {
            Code = "STD-GOODS",
            Description = "Standard Taxable Goods",
            TaxTreatment = TaxTreatment.Standard,
            ItemCategory = ItemCategory.GeneralGoods,
            CraReference = "Schedule VI, Part I",
            Notes = "Most goods and services fall into this category"
        };

        Console.WriteLine($"Tax Code: {standardGoods.Code}");
        Console.WriteLine($"Description: {standardGoods.Description}");
        Console.WriteLine($"Treatment: {standardGoods.TaxTreatment}");
        Console.WriteLine($"Taxable: {standardGoods.IsTaxable}");
        Console.WriteLine();

        // Zero-rated basic groceries
        var basicGroceries = new TaxCode
        {
            Code = "ZR-GROCERY",
            Description = "Basic Groceries",
            TaxTreatment = TaxTreatment.ZeroRated,
            ItemCategory = ItemCategory.BasicGroceries,
            CraReference = "Schedule VI, Part III",
            Notes = "Meat, fish, poultry, dairy, eggs, vegetables, fruits, cereals"
        };

        Console.WriteLine($"Tax Code: {basicGroceries.Code}");
        Console.WriteLine($"Description: {basicGroceries.Description}");
        Console.WriteLine($"Treatment: {basicGroceries.TaxTreatment}");
        Console.WriteLine($"Zero-Rated: {basicGroceries.IsZeroRated}");
        Console.WriteLine();

        // Exempt healthcare services
        var healthcareServices = new TaxCode
        {
            Code = "EX-HEALTH",
            Description = "Healthcare Services",
            TaxTreatment = TaxTreatment.Exempt,
            ItemCategory = ItemCategory.HealthcareServices,
            CraReference = "Schedule V, Part II",
            Notes = "Services rendered by healthcare professionals"
        };

        Console.WriteLine($"Tax Code: {healthcareServices.Code}");
        Console.WriteLine($"Description: {healthcareServices.Description}");
        Console.WriteLine($"Treatment: {healthcareServices.TaxTreatment}");
        Console.WriteLine($"Exempt: {healthcareServices.IsExempt}");
        Console.WriteLine();
    }

    /// <summary>
    /// Example: Creating customer tax profiles for different scenarios
    /// </summary>
    public static void CreateCustomerTaxProfiles()
    {
        Console.WriteLine("=== Customer Tax Profile Examples ===\n");

        // Regular consumer
        var regularCustomer = new CustomerTaxProfile
        {
            CustomerId = "CUST-001",
            RecipientStatus = RecipientStatus.Regular,
            TaxProvince = Provinces.BC,
            IsEligibleForExemption = false
        };

        Console.WriteLine($"Customer: {regularCustomer.CustomerId}");
        Console.WriteLine($"Status: {regularCustomer.RecipientStatus}");
        Console.WriteLine($"Province: {regularCustomer.TaxProvince}");
        Console.WriteLine($"Exempt: {regularCustomer.QualifiesForExemption(DateOnly.FromDateTime(DateTime.UtcNow))}");
        Console.WriteLine();

        // GST/HST Registrant
        var businessCustomer = new CustomerTaxProfile
        {
            CustomerId = "BUS-001",
            RecipientStatus = RecipientStatus.Registrant,
            GstHstNumber = "123456789RT0001",
            TaxProvince = Provinces.ON,
            IsEligibleForExemption = false // Still pays tax but can claim input credits
        };

        Console.WriteLine($"Customer: {businessCustomer.CustomerId}");
        Console.WriteLine($"Status: {businessCustomer.RecipientStatus}");
        Console.WriteLine($"GST/HST: {businessCustomer.GstHstNumber}");
        Console.WriteLine($"Exempt: {businessCustomer.QualifiesForExemption(DateOnly.FromDateTime(DateTime.UtcNow))}");
        Console.WriteLine();

        // First Nations exemption
        var firstNationsCustomer = new CustomerTaxProfile
        {
            CustomerId = "FN-001",
            RecipientStatus = RecipientStatus.FirstNations,
            ExemptionCertificateNumber = "FN-CERT-12345",
            TaxProvince = Provinces.BC,
            IsEligibleForExemption = true
        };

        Console.WriteLine($"Customer: {firstNationsCustomer.CustomerId}");
        Console.WriteLine($"Status: {firstNationsCustomer.RecipientStatus}");
        Console.WriteLine($"Certificate: {firstNationsCustomer.ExemptionCertificateNumber}");
        Console.WriteLine($"Exempt: {firstNationsCustomer.QualifiesForExemption(DateOnly.FromDateTime(DateTime.UtcNow))}");
        Console.WriteLine();
    }

    /// <summary>
    /// Example: Assigning tax codes to invoice items
    /// </summary>
    public static void AssignTaxCodesToItems()
    {
        Console.WriteLine("=== Item Classification Examples ===\n");

        // Create some sample items
        var groceryItemId = Guid.NewGuid();
        var softwareItemId = Guid.NewGuid();
        var healthServiceId = Guid.NewGuid();

        // Assign tax codes
        var groceryClassification = new ItemClassification
        {
            ItemId = groceryItemId,
            TaxCode = "ZR-GROCERY",
            AssignedBy = "Tax Administrator",
            Notes = "Basic grocery items - zero-rated under Schedule VI"
        };

        var softwareClassification = new ItemClassification
        {
            ItemId = softwareItemId,
            TaxCode = "STD-DIGITAL",
            AssignedBy = "Tax Administrator",
            Notes = "Digital services - standard GST/HST applies"
        };

        var healthClassification = new ItemClassification
        {
            ItemId = healthServiceId,
            TaxCode = "EX-HEALTH",
            AssignedBy = "Tax Administrator",
            Notes = "Healthcare services - exempt under Schedule V"
        };

        Console.WriteLine($"Grocery Item: {groceryClassification.TaxCode}");
        Console.WriteLine($"Software Item: {softwareClassification.TaxCode}");
        Console.WriteLine($"Health Service: {healthClassification.TaxCode}");
        Console.WriteLine();
    }

    /// <summary>
    /// Example: Demonstrating the complete tax calculation workflow
    /// </summary>
    public static void DemonstrateTaxCalculationWorkflow()
    {
        Console.WriteLine("=== Tax Calculation Workflow ===\n");

        // Create an invoice with mixed tax treatments
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-DEMO-001",
            CustomerName = "ABC Corporation",
            Province = Provinces.BC // 5% GST + 7% PST
        };

        // Add different types of items
        var standardProduct = new Product
        {
            Description = "Office Supplies",
            UnitPrice = 100.00m,
            Quantity = 1,
            TaxCategory = TaxCategory.TaxableProduct
        };

        var groceries = new Product
        {
            Description = "Basic Groceries",
            UnitPrice = 50.00m,
            Quantity = 1,
            TaxCategory = TaxCategory.NonTaxableProduct // Simulating zero-rated
        };

        invoice.AddItem(standardProduct);
        invoice.AddItem(groceries);

        // Apply taxes using the Canadian tax provider
        var taxProvider = new CanadianTaxProvider();
        var taxRates = taxProvider.GetTaxRates(invoice.Province, invoice.InvoiceDate);
        invoice.SetTaxRates(taxRates);

        // Display results
        Console.WriteLine($"Invoice: {invoice.InvoiceNumber}");
        Console.WriteLine($"Province: {invoice.Province} (GST + PST)");
        Console.WriteLine($"Subtotal: ${invoice.GetSubtotal():F2}");
        Console.WriteLine($"Tax: ${invoice.GetTotalTax():F2}");
        Console.WriteLine($"Total: ${invoice.GetTotal():F2}");
        Console.WriteLine();

        // Show item-by-item breakdown
        foreach (var item in invoice.Items)
        {
            Console.WriteLine($"  {item.Description}: ${item.GetSubtotal():F2} + ${item.GetTotalTax():F2} tax = ${item.GetTotal():F2}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Comprehensive demonstration of all tax code features
    /// </summary>
    public static void RunAllExamples()
    {
        try
        {
            CreateCanadianTaxCodes();
            CreateCustomerTaxProfiles();
            AssignTaxCodesToItems();
            DemonstrateTaxCalculationWorkflow();

            Console.WriteLine("=== All Examples Completed Successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running examples: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
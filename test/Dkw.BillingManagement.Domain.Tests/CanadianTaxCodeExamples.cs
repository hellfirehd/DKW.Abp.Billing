// DKW ABP Framework Extensions
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

namespace Dkw.BillingManagement;

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
            TaxProvince = TestData.BritishColumbia,
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
            TaxProvince = TestData.Ontario,
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
            TaxProvince = TestData.BritishColumbia,
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
        var groceryClassification = new TaxClassification
        {
            Id = groceryItemId,
            ItemId = groceryItemId,
            TaxCode = "ZR-GROCERY",
            AssignedBy = "Tax Administrator",
            Notes = "Basic grocery items - zero-rated under Schedule VI"
        };

        var softwareClassification = new TaxClassification
        {
            Id = softwareItemId,
            ItemId = softwareItemId,
            TaxCode = "STD-DIGITAL",
            AssignedBy = "Tax Administrator",
            Notes = "Digital services - standard GST/HST applies"
        };

        var healthClassification = new TaxClassification
        {
            Id = healthServiceId,
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
            Province = TestData.BritishColumbia // 5% GST + 7% PST
        };

        // Add different types of items
        var standardProduct = TestData.TaxableProductItem(TestData.EffectiveDate);

        var groceries = TestData.NonTaxableProductItem(TestData.EffectiveDate);

        invoice.AddItem(standardProduct);
        invoice.AddItem(groceries);

        // Apply taxes using the Canadian tax provider
        var taxProvider = TestData.TaxProvider;
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
            Console.WriteLine($"  {item.Description}: ${item.GetSubtotal():F2} + ${item.GetTaxTotal():F2} tax = ${item.GetTotal():F2}");
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

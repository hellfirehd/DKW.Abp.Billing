namespace Dkw.BillingManagement.EntityFrameworkCore;

public class FakeTaxCodeRepository : ITaxCodeRepository
{
    private readonly Dictionary<String, TaxCode> _taxCodes = new()
    {
        // Standard taxable goods
        ["STD-GOODS"] = new TaxCode
        {
            Code = "STD-GOODS",
            Description = "Standard Taxable Goods",
            TaxTreatment = TaxTreatment.Standard,
            ItemCategory = ItemCategory.GeneralGoods,
            CraReference = "Schedule VI, Part I"
        },

        // Basic groceries (zero-rated)
        ["ZR-GROCERY"] = new TaxCode
        {
            Code = "ZR-GROCERY",
            Description = "Basic Groceries",
            TaxTreatment = TaxTreatment.ZeroRated,
            ItemCategory = ItemCategory.BasicGroceries,
            CraReference = "Schedule VI, Part III",
            Notes = "Meat, fish, poultry, dairy, eggs, vegetables, fruits, cereals, etc."
        },

        // Prescription drugs (zero-rated)
        ["ZR-PRESCRIP"] = new TaxCode
        {
            Code = "ZR-PRESCRIP",
            Description = "Prescription Drugs",
            TaxTreatment = TaxTreatment.ZeroRated,
            ItemCategory = ItemCategory.PrescriptionDrugs,
            CraReference = "Schedule VI, Part II",
            Notes = "Drugs dispensed by prescription"
        },

        // Medical devices (zero-rated)
        ["ZR-MEDICAL"] = new TaxCode
        {
            Code = "ZR-MEDICAL",
            Description = "Medical Devices",
            TaxTreatment = TaxTreatment.ZeroRated,
            ItemCategory = ItemCategory.MedicalDevices,
            CraReference = "Schedule VI, Part II",
            Notes = "Prescribed medical devices and equipment"
        },

        // Healthcare services (exempt)
        ["EX-HEALTH"] = new TaxCode
        {
            Code = "EX-HEALTH",
            Description = "Healthcare Services",
            TaxTreatment = TaxTreatment.Exempt,
            ItemCategory = ItemCategory.HealthcareServices,
            CraReference = "Schedule V, Part II",
            Notes = "Services rendered by healthcare professionals"
        },

        // Educational services (exempt)
        ["EX-EDUCATION"] = new TaxCode
        {
            Code = "EX-EDUCATION",
            Description = "Educational Services",
            TaxTreatment = TaxTreatment.Exempt,
            ItemCategory = ItemCategory.EducationalServices,
            CraReference = "Schedule V, Part III",
            Notes = "Degree, diploma, certificate courses"
        },

        // Financial services (exempt)
        ["EX-FINANCIAL"] = new TaxCode
        {
            Code = "EX-FINANCIAL",
            Description = "Financial Services",
            TaxTreatment = TaxTreatment.Exempt,
            ItemCategory = ItemCategory.FinancialServices,
            CraReference = "Schedule V, Part VII",
            Notes = "Banking, insurance, investment services"
        },

        // Professional services (standard)
        ["STD-PROF"] = new TaxCode
        {
            Code = "STD-PROF",
            Description = "Professional Services",
            TaxTreatment = TaxTreatment.Standard,
            ItemCategory = ItemCategory.ProfessionalServices,
            Notes = "Consulting, accounting, legal services (non-exempt)"
        },

        // Digital services (standard)
        ["STD-DIGITAL"] = new TaxCode
        {
            Code = "STD-DIGITAL",
            Description = "Digital Services",
            TaxTreatment = TaxTreatment.Standard,
            ItemCategory = ItemCategory.DigitalServices,
            Notes = "Software, digital content, online services"
        },

        // Exports (zero-rated)
        ["ZR-EXPORT"] = new TaxCode
        {
            Code = "ZR-EXPORT",
            Description = "Exports",
            TaxTreatment = TaxTreatment.ZeroRated,
            ItemCategory = ItemCategory.Exports,
            CraReference = "Schedule VI, Part V",
            Notes = "Goods and services exported from Canada"
        }
    };

    public TaxCode GetByCode(String code)
    {
        return _taxCodes[code]
            ?? throw new KeyNotFoundException($"Tax code '{code}' not found.");
    }

    public IEnumerable<TaxCode> GetAll()
    {
        return _taxCodes.Values;
    }
    public void Add(TaxCode taxCode)
    {
        ArgumentNullException.ThrowIfNull(taxCode);

        _taxCodes.Add(taxCode.Code, taxCode);
    }

    public void Update(TaxCode taxCode)
    {
        ArgumentNullException.ThrowIfNull(taxCode);

        _taxCodes[taxCode.Code] = taxCode;
    }

    public void Delete(String code)
    {
        var taxCode = GetByCode(code);
        if (taxCode != null)
        {
            _taxCodes.Remove(taxCode.Code);
        }
    }
}

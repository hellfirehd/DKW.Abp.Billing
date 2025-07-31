using Billing;

namespace Billing.Domain.Tests;

/// <summary>
/// Tests for the enhanced Canadian tax code system
/// </summary>
public class EnhancedCanadianTaxSystemTests
{
    [Fact]
    public void TaxCode_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var taxCode = new TaxCode
        {
            Code = "ZR-GROCERY",
            Description = "Basic Groceries",
            TaxTreatment = TaxTreatment.ZeroRated,
            ItemCategory = ItemCategory.BasicGroceries,
            CraReference = "Schedule VI, Part III",
            Notes = "Basic food items"
        };

        // Assert
        Assert.Equal("ZR-GROCERY", taxCode.Code);
        Assert.Equal("Basic Groceries", taxCode.Description);
        Assert.Equal(TaxTreatment.ZeroRated, taxCode.TaxTreatment);
        Assert.Equal(ItemCategory.BasicGroceries, taxCode.ItemCategory);
        Assert.False(taxCode.IsTaxable); // Zero-rated is not taxable
        Assert.True(taxCode.IsZeroRated);
        Assert.False(taxCode.IsExempt);
        Assert.True(taxCode.IsValidOn(DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    [Fact]
    public void CustomerTaxProfile_ShouldHandleExemptions()
    {
        // Arrange
        var exemptProfile = new CustomerTaxProfile
        {
            CustomerId = "EXEMPT-001",
            RecipientStatus = RecipientStatus.FirstNations,
            IsEligibleForExemption = true,
            ExemptionCertificateNumber = "FN-12345"
        };

        var regularProfile = new CustomerTaxProfile
        {
            CustomerId = "REG-001",
            RecipientStatus = RecipientStatus.Regular,
            IsEligibleForExemption = false
        };

        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act & Assert
        Assert.True(exemptProfile.QualifiesForExemption(currentDate));
        Assert.False(regularProfile.QualifiesForExemption(currentDate));
    }

    [Fact]
    public void ItemClassification_ShouldLinkItemsToTaxCodes()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var classification = new ItemClassification
        {
            ItemId = itemId,
            TaxCode = "ZR-GROCERY",
            AssignedBy = "Tax Administrator",
            Notes = "Basic grocery item classification"
        };

        // Act & Assert
        Assert.Equal(itemId, classification.ItemId);
        Assert.Equal("ZR-GROCERY", classification.TaxCode);
        Assert.True(classification.IsValidOn(DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    [Theory]
    [InlineData(TaxTreatment.Standard, true, false, false)]
    [InlineData(TaxTreatment.ZeroRated, false, true, false)]
    [InlineData(TaxTreatment.Exempt, false, false, true)]
    [InlineData(TaxTreatment.OutOfScope, false, false, false)]
    public void TaxCode_ShouldHaveCorrectTreatmentFlags(TaxTreatment treatment, bool expectedTaxable, bool expectedZeroRated, bool expectedExempt)
    {
        // Arrange
        var taxCode = new TaxCode
        {
            TaxTreatment = treatment
        };

        // Act & Assert
        Assert.Equal(expectedTaxable, taxCode.IsTaxable);
        Assert.Equal(expectedZeroRated, taxCode.IsZeroRated);
        Assert.Equal(expectedExempt, taxCode.IsExempt);
    }

    [Fact]
    public void TaxCode_ShouldValidateDateRanges()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var taxCode = new TaxCode
        {
            EffectiveDate = currentDate,
            ExpirationDate = futureDate
        };

        // Act & Assert
        Assert.False(taxCode.IsValidOn(pastDate)); // Before effective date
        Assert.True(taxCode.IsValidOn(currentDate)); // On effective date
        Assert.True(taxCode.IsValidOn(futureDate)); // On expiration date
        Assert.False(taxCode.IsValidOn(futureDate.AddDays(1))); // After expiration
    }

    [Fact]
    public void RecipientStatus_ShouldHaveCorrectEnumValues()
    {
        // Test that the recipient status enum has all expected values
        Assert.True(Enum.IsDefined(typeof(RecipientStatus), RecipientStatus.Regular));
        Assert.True(Enum.IsDefined(typeof(RecipientStatus), RecipientStatus.Registrant));
        Assert.True(Enum.IsDefined(typeof(RecipientStatus), RecipientStatus.TaxExempt));
        Assert.True(Enum.IsDefined(typeof(RecipientStatus), RecipientStatus.FirstNations));
        Assert.True(Enum.IsDefined(typeof(RecipientStatus), RecipientStatus.NonResident));
        Assert.True(Enum.IsDefined(typeof(RecipientStatus), RecipientStatus.Government));
    }

    [Fact]
    public void ItemCategory_ShouldHaveCorrectEnumValues()
    {
        // Test key item categories
        Assert.True(Enum.IsDefined(typeof(ItemCategory), ItemCategory.GeneralGoods));
        Assert.True(Enum.IsDefined(typeof(ItemCategory), ItemCategory.BasicGroceries));
        Assert.True(Enum.IsDefined(typeof(ItemCategory), ItemCategory.PrescriptionDrugs));
        Assert.True(Enum.IsDefined(typeof(ItemCategory), ItemCategory.HealthcareServices));
        Assert.True(Enum.IsDefined(typeof(ItemCategory), ItemCategory.EducationalServices));
        Assert.True(Enum.IsDefined(typeof(ItemCategory), ItemCategory.FinancialServices));

        // Verify numeric values for organization
        Assert.Equal(100, (int)ItemCategory.GeneralGoods); // Goods start at 100
        Assert.Equal(200, (int)ItemCategory.GeneralServices); // Services start at 200
        Assert.Equal(300, (int)ItemCategory.Exports); // Special categories start at 300
    }

    [Fact]
    public void TaxTreatment_ShouldHaveCorrectEnumValues()
    {
        // Test all tax treatments
        Assert.True(Enum.IsDefined(typeof(TaxTreatment), TaxTreatment.Standard));
        Assert.True(Enum.IsDefined(typeof(TaxTreatment), TaxTreatment.ZeroRated));
        Assert.True(Enum.IsDefined(typeof(TaxTreatment), TaxTreatment.Exempt));
        Assert.True(Enum.IsDefined(typeof(TaxTreatment), TaxTreatment.OutOfScope));

        // Verify expected numeric values
        Assert.Equal(0, (int)TaxTreatment.Standard);
        Assert.Equal(1, (int)TaxTreatment.ZeroRated);
        Assert.Equal(2, (int)TaxTreatment.Exempt);
        Assert.Equal(3, (int)TaxTreatment.OutOfScope);
    }
}
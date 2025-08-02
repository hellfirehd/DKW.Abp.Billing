namespace Dkw.Abp.Billing;

/// <summary>
/// Tests for the enhanced Canadian tax code system
/// </summary>
public class EnumTests
{
    [Fact]
    public void RecipientStatus_ShouldHaveCorrectEnumValues()
    {
        // Test that the recipient status enum has all expected values
        Assert.True(Enum.IsDefined(RecipientStatus.Regular));
        Assert.True(Enum.IsDefined(RecipientStatus.Registrant));
        Assert.True(Enum.IsDefined(RecipientStatus.TaxExempt));
        Assert.True(Enum.IsDefined(RecipientStatus.FirstNations));
        Assert.True(Enum.IsDefined(RecipientStatus.NonResident));
        Assert.True(Enum.IsDefined(RecipientStatus.Government));
    }

    [Fact]
    public void ItemCategory_ShouldHaveCorrectEnumValues()
    {
        // Test key item categories
        Assert.True(Enum.IsDefined(ItemCategory.GeneralGoods));
        Assert.True(Enum.IsDefined(ItemCategory.BasicGroceries));
        Assert.True(Enum.IsDefined(ItemCategory.PrescriptionDrugs));
        Assert.True(Enum.IsDefined(ItemCategory.HealthcareServices));
        Assert.True(Enum.IsDefined(ItemCategory.EducationalServices));
        Assert.True(Enum.IsDefined(ItemCategory.FinancialServices));

        // Verify numeric values for organization
        Assert.Equal(100, (Int32)ItemCategory.GeneralGoods); // Goods start at 100
        Assert.Equal(200, (Int32)ItemCategory.GeneralServices); // Services start at 200
        Assert.Equal(300, (Int32)ItemCategory.Exports); // Special categories start at 300
    }

    [Fact]
    public void TaxTreatment_ShouldHaveCorrectEnumValues()
    {
        // Test all tax treatments
        Assert.True(Enum.IsDefined(TaxTreatment.Standard));
        Assert.True(Enum.IsDefined(TaxTreatment.ZeroRated));
        Assert.True(Enum.IsDefined(TaxTreatment.Exempt));
        Assert.True(Enum.IsDefined(TaxTreatment.OutOfScope));

        // Verify expected numeric values
        Assert.Equal(0, (Int32)TaxTreatment.Standard);
        Assert.Equal(1, (Int32)TaxTreatment.ZeroRated);
        Assert.Equal(2, (Int32)TaxTreatment.Exempt);
        Assert.Equal(3, (Int32)TaxTreatment.OutOfScope);
    }
}

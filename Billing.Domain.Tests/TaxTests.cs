namespace Billing.Domain.Tests;

public class TaxTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var tax = new Tax("GST", "Goods and Services Tax", TaxJurisdiction.Federal);

        // Assert
        Assert.Equal("GST", tax.Code);
        Assert.Equal("Goods and Services Tax", tax.Name);
        Assert.Empty(tax.Rates);
    }

    [Fact]
    public void AddTaxRate_AddsRateToCollection()
    {
        // Arrange
        var tax = new Tax("GST", "Goods and Services Tax", TaxJurisdiction.Federal);
        var effectiveDate = new DateOnly(2020, 1, 1);
        var expirationDate = new DateOnly(2020, 12, 31);

        // Act
        tax.AddTaxRate(0.1m, effectiveDate, expirationDate);

        // Assert
        Assert.Single(tax.Rates);
        var addedRate = tax.Rates.First();
        Assert.Equal(0.1m, addedRate.Rate);
        Assert.Equal(effectiveDate, addedRate.EffectiveDate);
        Assert.Equal(expirationDate, addedRate.ExpirationDate);
    }

    [Fact]
    public void AddTaxRate_ReturnsSameInstance_ForChaining()
    {
        // Arrange
        var tax = new Tax("GST", "Goods and Services Tax", TaxJurisdiction.Federal);

        // Act
        var result = tax.AddTaxRate(0.1m, new DateOnly(2020, 1, 1));

        // Assert
        Assert.Same(tax, result);
    }

    [Fact]
    public void GetTaxRate_ReturnsNull_WhenNoRatesExist()
    {
        // Arrange
        var tax = new Tax("GST", "Goods and Services Tax", TaxJurisdiction.Federal);
        var date = new DateOnly(2020, 6, 1);

        // Act
        var result = tax.GetTaxRate(date);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetTaxRate_ReturnsRate_WhenDateIsWithinRange()
    {
        // Arrange
        var tax = new Tax("GST", "Goods and Services Tax", TaxJurisdiction.Federal);
        var effectiveDate = new DateOnly(2020, 1, 1);
        var expirationDate = new DateOnly(2020, 12, 31);
        tax.AddTaxRate(0.1m, effectiveDate, expirationDate);
        var date = new DateOnly(2020, 6, 1);

        // Act
        var result = tax.GetTaxRate(date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0.1m, result.Rate);
    }

    [Fact]
    public void GetTaxRate_ReturnsNull_WhenDateIsOutsideRange()
    {
        // Arrange
        var tax = new Tax("GST", "Goods and Services Tax", TaxJurisdiction.Federal);
        tax.AddTaxRate(0.1m, new DateOnly(2020, 1, 1), new DateOnly(2020, 12, 31));
        var date = new DateOnly(2021, 6, 1);

        // Act
        var result = tax.GetTaxRate(date);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetTaxRate_ReturnsMostRecentRate_WhenMultipleRatesMatch()
    {
        // Arrange
        var tax = new Tax("GST", "Goods and Services Tax", TaxJurisdiction.Federal);
        tax.AddTaxRate(0.1m, new DateOnly(2019, 1, 1), new DateOnly(2022, 12, 31));
        tax.AddTaxRate(0.15m, new DateOnly(2020, 1, 1), new DateOnly(2021, 12, 31));
        var date = new DateOnly(2020, 6, 1);

        // Act
        var result = tax.GetTaxRate(date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0.15m, result.Rate);
    }

    [Fact]
    public void GetTaxRate_HandlesNullExpirationDate()
    {
        // Arrange
        var tax = new Tax("GST", "Goods and Services Tax", TaxJurisdiction.Federal);
        tax.AddTaxRate(0.1m, new DateOnly(2020, 1, 1));
        var date = new DateOnly(2023, 6, 1);

        // Act
        var result = tax.GetTaxRate(date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0.1m, result.Rate);
    }

    [Fact]
    public void Name_CanBeModified()
    {
        // Arrange
        var tax = new Tax("GST", "Goods and Services Tax", TaxJurisdiction.Federal)
        {
            // Act
            Name = "Value Added Tax"
        };

        // Assert
        Assert.Equal("Value Added Tax", tax.Name);
    }
}

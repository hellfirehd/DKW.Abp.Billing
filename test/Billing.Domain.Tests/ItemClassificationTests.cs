namespace Dkw.Abp.Billing;

public class ItemClassificationTests
{
    [Fact]
    public void ItemClassification_ShouldLinkItemsToTaxCodes()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var classification = new TaxClassification
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            TaxCode = "ZR-GROCERY",
            AssignedBy = "Tax Administrator",
            Notes = "Basic grocery item classification",
            AssignedDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // Act & Assert
        Assert.Equal(itemId, classification.ItemId);
        Assert.Equal("ZR-GROCERY", classification.TaxCode);
        Assert.True(classification.IsValidOn(DateOnly.FromDateTime(DateTime.UtcNow)));
    }
}

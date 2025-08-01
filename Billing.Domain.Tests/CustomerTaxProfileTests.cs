namespace Billing.Domain.Tests;

public class CustomerTaxProfileTests
{

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
}

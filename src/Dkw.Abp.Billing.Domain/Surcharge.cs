namespace Dkw.Abp.Billing;

/// <summary>
/// Represents a surcharge applied to an order
/// </summary>
public class Surcharge
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public String Name { get; set; } = String.Empty;
    public String Description { get; set; } = String.Empty;
    public SurchargeType Type { get; set; }
    public Decimal FixedAmount { get; set; }
    public Decimal PercentageRate { get; set; } // As Decimal (e.g., 0.029 for 2.9%)
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Calculates the surcharge amount based on the order total
    /// </summary>
    public Decimal CalculateSurchargeAmount(Decimal orderTotal)
    {
        if (!IsActive)
        {
            return 0;
        }

        var surchargeAmount = FixedAmount + orderTotal * PercentageRate;
        return Math.Round(surchargeAmount, 2);
    }
}

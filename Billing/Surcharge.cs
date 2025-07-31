namespace Billing;

/// <summary>
/// Represents a surcharge applied to an order
/// </summary>
public class Surcharge
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SurchargeType Type { get; set; }
    public decimal FixedAmount { get; set; }
    public decimal PercentageRate { get; set; } // As decimal (e.g., 0.029 for 2.9%)
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Calculates the surcharge amount based on the order total
    /// </summary>
    public decimal CalculateSurchargeAmount(decimal orderTotal)
    {
        if (!IsActive)
        {
            return 0;
        }

        decimal surchargeAmount = FixedAmount + (orderTotal * PercentageRate);
        return Math.Round(surchargeAmount, 2);
    }
}

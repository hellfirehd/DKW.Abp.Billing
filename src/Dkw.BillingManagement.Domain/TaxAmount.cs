namespace Dkw.BillingManagement;

/// <summary>
/// Represents a calculated tax amount for an invoice item
/// </summary>
public class TaxAmount
{
    public TaxRate TaxRate { get; set; } = null!;
    public String Description => TaxRate.Name;
    public Decimal TaxableAmount { get; set; }
    public Decimal Amount { get; set; }
}

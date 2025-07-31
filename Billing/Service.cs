namespace Billing;

/// <summary>
/// Represents a service that can be billed
/// </summary>
public class Service : InvoiceItem
{
    public decimal HourlyRate { get; set; }
    public decimal Hours { get; set; }

    public override decimal GetSubtotal() => HourlyRate * Hours;
}
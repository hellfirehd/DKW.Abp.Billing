namespace Billing;

/// <summary>
/// Represents a service that can be sold
/// </summary>
public class Service : InvoiceItem
{
    public decimal HourlyRate 
    { 
        get => UnitPrice; 
        set => UnitPrice = value; 
    }
    
    public decimal Hours 
    { 
        get => Quantity; 
        set => Quantity = (int)Math.Round(value, 2);
    }
    
    public string ServiceType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public DateOnly? ServiceDate { get; set; }
    public TimeSpan? Duration { get; set; }
    
    public Service()
    {
        UnitType = "Hour";
        TaxCategory = TaxCategory.TaxableService;
    }
    
    /// <summary>
    /// Gets the subtotal for service (rate * hours)
    /// </summary>
    public override decimal GetSubtotal() => HourlyRate * Hours;
}


namespace Billing;

/// <summary>
/// Represents a tax that changes over time, such as sales tax or VAT.
/// </summary>
/// <param name="code"></param>
/// <param name="name"></param>
/// <param name="scope"></param>
/// <param name="rates"></param>
public class Tax(String code, String name, TaxScope scope, IReadOnlyCollection<TaxRate> rates)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public TaxScope Scope { get; set; } = scope;
    public IReadOnlyCollection<TaxRate> Rates { get; set; } = rates;

    /// <summary>
    /// Indicates if this is a harmonized sales tax (combines GST and PST)
    /// </summary>
    public bool IsHarmonized => Code.Contains("HST");

    /// <summary>
    /// Indicates if this is a federal tax (GST)
    /// </summary>
    public bool IsFederal => Scope == TaxScope.Federal;

    /// <summary>
    /// Indicates if this is a provincial tax (PST, QST, RST)
    /// </summary>
    public bool IsProvincial => Scope == TaxScope.Provincial;

    public TaxRate? TaxRate(DateOnly date)
        => Rates.OrderByDescending(r => r.EffectiveDate)
        .FirstOrDefault(tr => tr.EffectiveDate <= date && date <= tr.ExpirationDate);
}

/// <summary>
/// Represents a tax configuration for a specific province and item category
/// </summary>
public class TaxRate(DateOnly effectiveDate, DateOnly? expirationDate, decimal rate)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TaxCategory TaxCategory { get; set; } = TaxCategory.TaxableProduct;
    public decimal Rate { get; set; } = rate;
    public bool IsActive { get; set; } = true;
    public DateOnly EffectiveDate { get; set; } = effectiveDate;
    public DateOnly? ExpirationDate { get; set; } = expirationDate;
}

/// <summary>
/// Represents a calculated tax amount for an invoice item
/// </summary>
public class TaxAmount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public TaxRate TaxRate { get; set; } = null!;
    public decimal TaxableAmount { get; set; }
    public decimal Amount { get; set; }
    public string Description => TaxRate.Name;
}

public class TaxContainer
{
    public Tax GST { get; set; } = new Tax("GST", "Goods and Services Tax", TaxScope.Federal, []);

    public TaxRate BC_PST { get; set; } = new TaxRate(new DateOnly(2013, 4, 1), null, 0.07M);

}
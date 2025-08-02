using System.Diagnostics;

namespace Billing;

/// <summary>
/// Represents a tax rate for a specific date range, 
/// </summary>
[DebuggerDisplay("{ToString()}")]
public record TaxRate
{
    private TaxRate()
    {
        // Parameterless constructor for EF Core/JSON serialization
    }

    public TaxRate(Tax tax, Decimal rate, DateOnly effectiveDate, DateOnly? expirationDate = null)
    {
        Tax = tax ?? throw new ArgumentNullException(nameof(tax));
        Rate = rate < 0
        ? throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be non-negative.")
        : rate;

        EffectiveDate = effectiveDate < DateOnly.FromDateTime(DateTime.UnixEpoch)
            ? throw new ArgumentOutOfRangeException(nameof(effectiveDate), "Effective Date is too far in the past.")
            : effectiveDate;

        ExpirationDate = expirationDate;
    }

    public Tax Tax { get; } = default!;
    public Decimal Rate { get; init; } // Allow init to facilitate Zero-Rated tax rates.

    public Boolean IsActive { get; init; } = true;
    public DateOnly EffectiveDate { get; }
    public DateOnly? ExpirationDate { get; }

    public String Name => Tax.Name;
    public String Code => Tax.Code;

    public Boolean InEffect(DateOnly date)
        => EffectiveDate <= date
        && (!ExpirationDate.HasValue || date <= ExpirationDate.Value);

    public override String ToString()
        => $"Tax: {Name}, Rate: {Rate}, Active: {IsActive}, Effective: {EffectiveDate}, Expiration: {ExpirationDate?.ToString() ?? "N/A"}";

    public static TaxRate Create(Tax tax, Decimal rate, DateOnly effectiveDate, DateOnly? expirationDate = null)
        => new(tax, rate, effectiveDate, expirationDate);
}

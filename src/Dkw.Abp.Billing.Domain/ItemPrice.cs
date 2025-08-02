namespace Dkw.Abp.Billing;

/// <summary>
/// Represents the per unit price of an item and the date range during which the price is in effect.
/// </summary>
/// <remarks>
/// This record is used to define the pricing details for an item. It must have a start date of when
/// the price goes into effect and an optional expiration date of when the price is not longer in effect.
/// The <see cref="InEffect(DateOnly)"/> method can be used to determine whether the price is valid for
/// a specific date.
/// </remarks>
/// <param name="UnitPrice"></param>
/// <param name="EffectiveDate"></param>
/// <param name="ExpirationDate"></param>
public record ItemPrice(Decimal UnitPrice, DateOnly EffectiveDate, DateOnly? ExpirationDate = null, Boolean IsActive = true)
{
    public bool InEffect(DateOnly date)
        => EffectiveDate <= date
        && (!ExpirationDate.HasValue || date <= ExpirationDate.Value);
}
namespace Billing;

public interface ITaxProvider
{
    ItemTaxRate[] GetTaxRates(Province province, DateOnly effectiveDate);
}

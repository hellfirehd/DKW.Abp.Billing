namespace Billing;

public class BillingSettings
{
    public Dictionary<string, List<TaxRateConfig>> TaxRatesByState { get; set; }
    public SurchargeConfig DefaultCreditCardSurcharge { get; set; }
    public int DefaultInvoiceDueDays { get; set; }
}
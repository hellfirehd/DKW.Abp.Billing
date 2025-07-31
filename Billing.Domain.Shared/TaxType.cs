namespace Billing;

public enum TaxType
{
    None = 0,

    Goods = 1, // GST

    Services = 2, // PST, QST

    Combined = 4 // HST
}

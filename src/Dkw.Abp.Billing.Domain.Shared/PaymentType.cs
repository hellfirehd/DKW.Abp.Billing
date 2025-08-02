using System.ComponentModel;

namespace Dkw.Abp.Billing;

/// <summary>
/// Represents different types of payment methods
/// </summary>
public enum PaymentType
{
    [Description("Credit Card")]
    CreditCard,

    [Description("Bank Transfer")]
    BankTransfer,

    [Description("Cash")]
    Cash,

    [Description("Check")]
    Check,

    [Description("Other")]
    Other
}

using System.ComponentModel;

namespace Dkw.Abp.Billing;

/// <summary>
/// Represents different types of surcharges
/// </summary>
public enum SurchargeType
{
    [Description("Credit Card Processing")]
    CreditCardProcessing,

    [Description("Convenience Fee")]
    ConvenienceFee,

    [Description("Service Fee")]
    ServiceFee
}

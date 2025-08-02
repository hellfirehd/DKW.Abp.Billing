using System.ComponentModel;

namespace Dkw.BillingManagement;

/// <summary>
/// Represents the status of a payment
/// </summary>
public enum PaymentStatus
{
    [Description("Pending")]
    Pending,

    [Description("Completed")]
    Completed,

    [Description("Failed")]
    Failed,

    [Description("Cancelled")]
    Cancelled,

    [Description("Refunded")]
    Refunded,

    [Description("Partially Refunded")]
    PartiallyRefunded
}

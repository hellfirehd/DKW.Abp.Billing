using System.ComponentModel;

namespace Billing;

/// <summary>
/// Represents the status of an invoice
/// </summary>
public enum InvoiceStatus
{
    [Description("Draft")]
    Draft,

    [Description("Pending")]
    Pending,

    [Description("Sent")]
    Sent,

    [Description("Paid")]
    Paid,

    [Description("Overdue")]
    Overdue,

    [Description("Cancelled")]
    Cancelled,

    [Description("Refunded")]
    Refunded,

    [Description("Partially Refunded")]
    PartiallyRefunded
}

namespace Billing;

/// <summary>
/// Represents a refund processed against payments
/// </summary>
public class Refund
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Amount { get; set; }
    public DateOnly RefundDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public string Reason { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // Link to original payment if applicable
    public string? OriginalPaymentId { get; set; }

    // For tracking gateway information
    public string GatewayTransactionId { get; set; } = string.Empty;
    public string GatewayName { get; set; } = string.Empty;

    // Breakdown of what was refunded (proportional tax/discount adjustment)
    public decimal SubtotalRefund { get; set; }
    public decimal TaxRefund { get; set; }
    public decimal ShippingRefund { get; set; }
    // Note: Surcharges are typically not refundable

    /// <summary>
    /// Creates a refund with proper breakdown based on invoice totals
    /// </summary>
    public static Refund CreateProportionalRefund(
        Invoice invoice,
        decimal refundAmount,
        string reason,
        string? originalPaymentId = null)
    {
        if (refundAmount <= 0)
        {
            throw new ArgumentException("Refund amount must be positive", nameof(refundAmount));
        }

        var invoiceTotal = invoice.GetTotal();
        if (refundAmount > invoiceTotal)
        {
            throw new ArgumentException("Refund amount cannot exceed invoice total", nameof(refundAmount));
        }

        var proportion = refundAmount / invoiceTotal;

        return new Refund
        {
            Amount = refundAmount,
            Reason = reason,
            OriginalPaymentId = originalPaymentId,
            SubtotalRefund = Math.Round(invoice.GetTotalAfterDiscounts() * proportion, 2),
            TaxRefund = Math.Round(invoice.GetTotalTax() * proportion, 2),
            ShippingRefund = Math.Round(invoice.ShippingCost * proportion, 2)
        };
    }
}
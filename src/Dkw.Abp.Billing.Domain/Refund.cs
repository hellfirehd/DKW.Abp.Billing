namespace Dkw.Abp.Billing;

/// <summary>
/// Represents a refund processed against payments
/// </summary>
public class Refund
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Decimal Amount { get; set; }
    public DateOnly RefundDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public String Reason { get; set; } = String.Empty;
    public String ReferenceNumber { get; set; } = String.Empty;
    public String Notes { get; set; } = String.Empty;

    // Link to original payment if applicable
    public String? OriginalPaymentId { get; set; }

    // For tracking gateway information
    public String GatewayTransactionId { get; set; } = String.Empty;
    public String GatewayName { get; set; } = String.Empty;

    // Breakdown of what was refunded (proportional tax/discount adjustment)
    public Decimal SubtotalRefund { get; set; }
    public Decimal TaxRefund { get; set; }
    public Decimal ShippingRefund { get; set; }

    // Note: Currently, surcharges are not refundable

    /// <summary>
    /// Creates a refund with proper breakdown based on invoice totals
    /// </summary>
    public static Refund CreateProportionalRefund(
        Invoice invoice,
        Decimal refundAmount,
        String reason,
        String? originalPaymentId = null)
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
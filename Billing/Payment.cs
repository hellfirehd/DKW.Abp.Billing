namespace Billing;

/// <summary>
/// Represents a payment made against an invoice
/// </summary>
public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Decimal Amount { get; set; }
    public IPaymentMethod Method { get; set; } = default!;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateOnly PaymentDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public String ReferenceNumber { get; set; } = String.Empty;
    public String Notes { get; set; } = String.Empty;

    // For tracking gateway information
    public String GatewayTransactionId { get; set; } = String.Empty;
    public String GatewayName { get; set; } = String.Empty;

    /// <summary>
    /// Marks the payment as completed
    /// </summary>
    public void MarkAsCompleted(String? transactionId = null)
    {
        Status = PaymentStatus.Completed;
        if (!String.IsNullOrEmpty(transactionId))
        {
            GatewayTransactionId = transactionId;
        }
    }

    /// <summary>
    /// Marks the payment as failed
    /// </summary>
    public void MarkAsFailed(String reason = "")
    {
        Status = PaymentStatus.Failed;
        if (!String.IsNullOrEmpty(reason))
        {
            Notes = reason;
        }
    }

    /// <summary>
    /// Cancels the payment
    /// </summary>
    public void Cancel(String reason = "")
    {
        if (Status == PaymentStatus.Pending)
        {
            Status = PaymentStatus.Cancelled;
            if (!String.IsNullOrEmpty(reason))
            {
                Notes = reason;
            }
        }
        else
        {
            throw new InvalidOperationException($"Cannot cancel payment with status: {Status}");
        }
    }
}
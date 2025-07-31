namespace Billing;

/// <summary>
/// Represents a payment made against an invoice
/// </summary>
public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Amount { get; set; }
    public IPaymentMethod Method { get; set; } = default!;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateOnly PaymentDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // For tracking gateway information
    public string GatewayTransactionId { get; set; } = string.Empty;
    public string GatewayName { get; set; } = string.Empty;

    /// <summary>
    /// Marks the payment as completed
    /// </summary>
    public void MarkAsCompleted(string? transactionId = null)
    {
        Status = PaymentStatus.Completed;
        if (!string.IsNullOrEmpty(transactionId))
        {
            GatewayTransactionId = transactionId;
        }
    }

    /// <summary>
    /// Marks the payment as failed
    /// </summary>
    public void MarkAsFailed(string reason = "")
    {
        Status = PaymentStatus.Failed;
        if (!string.IsNullOrEmpty(reason))
        {
            Notes = reason;
        }
    }

    /// <summary>
    /// Cancels the payment
    /// </summary>
    public void Cancel(string reason = "")
    {
        if (Status == PaymentStatus.Pending)
        {
            Status = PaymentStatus.Cancelled;
            if (!string.IsNullOrEmpty(reason))
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
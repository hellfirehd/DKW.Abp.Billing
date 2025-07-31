namespace Billing;

public class RefundResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateOnly RefundDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? OriginalPaymentId { get; set; }
}

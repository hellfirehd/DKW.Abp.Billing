namespace Billing;

public class CreateRefundRequest
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? OriginalPaymentId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

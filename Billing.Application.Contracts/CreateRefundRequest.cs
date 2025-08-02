namespace Billing;

public class CreateRefundRequest
{
    public Guid InvoiceId { get; set; }
    public Decimal Amount { get; set; }
    public String Reason { get; set; } = String.Empty;
    public String? OriginalPaymentId { get; set; }
    public String? ReferenceNumber { get; set; }
    public String? Notes { get; set; }
}

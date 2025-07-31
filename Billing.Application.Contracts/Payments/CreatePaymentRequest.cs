namespace Billing.Payments;

public class CreatePaymentRequest
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public IPaymentMethod Method { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string? GatewayTransactionId { get; set; }
    public string? GatewayName { get; set; }
}

namespace Billing;

public class CreatePaymentRequest
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public IPaymentMethod Method { get; set; } = default!;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string? GatewayTransactionId { get; set; }
    public string? GatewayName { get; set; }
}

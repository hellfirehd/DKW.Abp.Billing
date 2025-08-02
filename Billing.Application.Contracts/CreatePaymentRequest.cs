namespace Billing;

public class CreatePaymentRequest
{
    public Guid InvoiceId { get; set; }
    public Decimal Amount { get; set; }
    public IPaymentMethod Method { get; set; } = default!;
    public String ReferenceNumber { get; set; } = String.Empty;
    public String Notes { get; set; } = String.Empty;
    public String? GatewayTransactionId { get; set; }
    public String? GatewayName { get; set; }
}

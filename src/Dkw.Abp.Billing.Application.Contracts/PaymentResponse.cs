namespace Dkw.Abp.Billing;

public class PaymentResponse
{
    public Guid Id { get; set; }
    public Decimal Amount { get; set; }
    public IPaymentMethod Method { get; set; } = default!;
    public PaymentStatus Status { get; set; }
    public DateOnly PaymentDate { get; set; }
    public String ReferenceNumber { get; set; } = String.Empty;
    public String Notes { get; set; } = String.Empty;
}

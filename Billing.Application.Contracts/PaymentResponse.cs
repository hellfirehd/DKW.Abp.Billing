namespace Billing;

public class PaymentResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public IPaymentMethod Method { get; set; } = default!;
    public PaymentStatus Status { get; set; }
    public DateOnly PaymentDate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

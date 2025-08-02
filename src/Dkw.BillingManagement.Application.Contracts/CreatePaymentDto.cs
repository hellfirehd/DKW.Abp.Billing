namespace Dkw.BillingManagement;

/// <summary>
/// DTO for payment information
/// </summary>
public class CreatePaymentDto
{
    public Guid InvoiceId { get; set; }
    public Decimal Amount { get; set; }
    public IPaymentMethod Method { get; set; } = default!;
    public String ReferenceNumber { get; set; } = String.Empty;
    public String Notes { get; set; } = String.Empty;
}

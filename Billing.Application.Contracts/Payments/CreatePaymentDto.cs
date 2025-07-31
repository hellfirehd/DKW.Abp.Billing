namespace Billing.Payments;

/// <summary>
/// DTO for payment information
/// </summary>
public class CreatePaymentDto
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public IPaymentMethod Method { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

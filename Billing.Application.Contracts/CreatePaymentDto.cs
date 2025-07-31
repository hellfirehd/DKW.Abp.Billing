namespace Billing;

/// <summary>
/// DTO for payment information
/// </summary>
public class CreatePaymentDto
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public IPaymentMethod Method { get; set; } = default!;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

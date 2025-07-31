namespace Billing;

public class InvoicePaidEvent
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly PaidDate { get; set; }
}

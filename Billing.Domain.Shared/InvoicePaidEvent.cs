namespace Billing;

public class InvoicePaidEvent
{
    public Guid InvoiceId { get; set; }
    public Decimal Amount { get; set; }
    public DateOnly PaidDate { get; set; }
}

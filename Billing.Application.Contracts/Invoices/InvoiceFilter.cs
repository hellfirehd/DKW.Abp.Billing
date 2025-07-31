namespace Billing.Invoices;

public class InvoiceFilter
{
    public InvoiceStatus? Status { get; set; }
    public string? CustomerName { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
}

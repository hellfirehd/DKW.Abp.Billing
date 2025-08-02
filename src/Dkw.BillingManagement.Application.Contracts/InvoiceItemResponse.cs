namespace Dkw.BillingManagement;

public class InvoiceItemResponse
{
    public Guid Id { get; set; }
    public String Description { get; set; } = String.Empty;
    public Decimal UnitPrice { get; set; }
    public Decimal Quantity { get; set; }
    public Decimal Subtotal { get; set; }
    public Decimal TotalTax { get; set; }
    public Decimal TotalDiscount { get; set; }
    public Decimal Total { get; set; }
    public ItemType ItemType { get; set; }
}

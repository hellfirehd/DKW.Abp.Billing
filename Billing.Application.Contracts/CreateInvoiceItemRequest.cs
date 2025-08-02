namespace Billing;

public class CreateInvoiceItemRequest
{
    public required Guid ItemId { get; set; }

    // Product specific fields
    public Decimal Quantity { get; set; } = 1.0m;
}

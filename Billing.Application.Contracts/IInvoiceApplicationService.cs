namespace Billing;

/// <summary>
/// Application service contract for invoice operations
/// </summary>
public interface IInvoiceApplicationService
{
    /// <summary>
    /// Creates a new invoice
    /// </summary>
    Task<Guid> CreateInvoiceAsync(CreateInvoiceRequest request);

    /// <summary>
    /// Gets an invoice by ID
    /// </summary>
    Task<InvoiceResponse?> GetInvoiceAsync(Guid invoiceId);

    /// <summary>
    /// Updates an invoice
    /// </summary>
    Task UpdateInvoiceAsync(Guid invoiceId, UpdateInvoiceRequest request);

    /// <summary>
    /// Adds an item to an invoice
    /// </summary>
    Task AddInvoiceItemAsync(Guid invoiceId, CreateInvoiceItemRequest request);

    /// <summary>
    /// Removes an item from an invoice
    /// </summary>
    Task RemoveInvoiceItemAsync(Guid invoiceId, string itemId);

    /// <summary>
    /// Applies discounts to an invoice
    /// </summary>
    Task ApplyDiscountsAsync(Guid invoiceId, List<string> discountIds);

    /// <summary>
    /// Applies surcharges to an invoice
    /// </summary>
    Task ApplySurchargesAsync(Guid invoiceId, List<string> surchargeIds);

    /// <summary>
    /// Sets tax rates for an invoice based on state and item categories
    /// </summary>
    Task SetTaxRatesAsync(Guid invoiceId);

    /// <summary>
    /// Marks an invoice as sent
    /// </summary>
    Task MarkInvoiceAsSentAsync(Guid invoiceId);

    /// <summary>
    /// Cancels an invoice
    /// </summary>
    Task CancelInvoiceAsync(Guid invoiceId);

    /// <summary>
    /// Processes a payment for an invoice
    /// </summary>
    Task<Guid> ProcessPaymentAsync(CreatePaymentRequest request);

    /// <summary>
    /// Processes a refund for an invoice
    /// </summary>
    Task<Guid> ProcessRefundAsync(CreateRefundRequest request);

    /// <summary>
    /// Gets all invoices with optional filtering
    /// </summary>
    Task<List<InvoiceResponse>> GetInvoicesAsync(InvoiceFilter? filter = null);

    Task MoveItemUp(Guid invoiceId, string itemId);
    Task MoveItemDown(Guid invoiceId, string itemId);
}

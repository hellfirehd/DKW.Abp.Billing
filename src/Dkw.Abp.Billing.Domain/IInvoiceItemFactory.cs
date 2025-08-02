namespace Dkw.Abp.Billing;

public interface IInvoiceItemFactory
{
    /// <summary>
    /// Determines whether the specified item can be created by this instance.
    /// </summary>
    /// <param name="item">The item to evaluate for creation eligibility. Cannot be null.</param>
    /// <returns><see langword="true"/> if the item meets the criteria for creation; otherwise, <see langword="false"/>.</returns>
    Boolean CanCreate(Item item);

    /// <summary>
    /// Creates an invoice item based on the provided request.
    /// </summary>
    /// <param name="item">The request containing details for the invoice item.</param>
    /// <param name="quantity">The quantity of the item.</param>
    /// <param name="invoiceDate">The date used to determine the unit price.</param>
    /// <returns>An instance of <see cref="InvoiceItem"/>.</returns>
    InvoiceItem Create(Item item, DateOnly invoiceDate);
}

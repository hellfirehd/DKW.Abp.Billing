// DKW Billing Management
// Copyright (C) 2025 Doug Wilson
//
// This program is free software: you can redistribute it and/or modify it under the terms of
// the GNU Affero General Public License as published by the Free Software Foundation, either
// version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License along with this
// program. If not, see <https://www.gnu.org/licenses/>.

using Dkw.BillingManagement.Customers;
using Dkw.BillingManagement.Invoices.LineItems;
using Dkw.BillingManagement.Items;
using Dkw.BillingManagement.Payments;
using Dkw.BillingManagement.Provinces;
using Dkw.BillingManagement.Taxes;

namespace Dkw.BillingManagement.Invoices;

/// <summary>
/// Application service for invoice operations
/// </summary>
public class InvoiceApplicationService(
    ICustomerManager customerManager,
    IInvoiceManager invoiceManager,
    ILineItemFactory lineItemFactory,
    IPaymentMethodRepository paymentMethodRepository,
    IProvinceRepository provinceManager,
    IItemRepository itemRepository,
    ITaxManager taxProvider)
    : BillingManagementAppService, IInvoiceApplicationService
{
    public ICustomerManager CustomerManager { get; } = customerManager;
    public IInvoiceManager InvoiceManager { get; } = invoiceManager;
    public ILineItemFactory LineItemFactory { get; } = lineItemFactory;
    public IPaymentMethodRepository PaymentMethodRepository { get; } = paymentMethodRepository;
    public IProvinceRepository ProvinceRepository { get; } = provinceManager;
    public IItemRepository ItemRepository { get; } = itemRepository;
    private ITaxManager TaxProvider { get; } = taxProvider;

    /// <summary>
    /// Creates a new invoice, returning the ID of the created invoice.
    /// </summary>
    public async Task<Result<Guid>> CreateInvoiceAsync(CreateInvoice command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var customer = await CustomerManager.GetCustomerAsync(command.CustomerId, cancellationToken);

        var lineItems = await CreateLineItemsAsync(command.InvoiceDate, command.Items, cancellationToken);

        var invoiceId = await InvoiceManager.CreateInvoiceAsync(customer, lineItems, cancellationToken);

        return Result.Success(invoiceId);
    }

    /// <summary>
    /// Gets an invoice by ID
    /// </summary>
    public async Task<InvoiceDto> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await GetInvoiceOrThrowAsync(invoiceId, cancellationToken);

        var response = ObjectMapper.Map<Invoice, InvoiceDto>(invoice);

        return response;
    }

    public Task<IEnumerable<InvoiceDto>> GetInvoicesAsync(InvoiceFilter filter, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<Result> AddDiscountsAsync(AddDiscount command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<Result> AddSurchargesAsync(AddSurcharge command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<Result> CancelInvoiceAsync(CancelInvoice command, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<Result> PostInvoiceAsync(PostInvoice command, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <summary>
    /// Adds an item to an invoice
    /// </summary>
    public async Task<Result> AddLineItemAsync(AddLineItem command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invoice = await GetInvoiceOrThrowAsync(command.InvoiceId, cancellationToken);

        var lineItem = await CreateLineItemAsync(invoice.InvoiceDate, command.ItemId, command.Quantity, cancellationToken);

        invoice.AddLineItem(lineItem);

        await InvoiceManager.UpdateInvoiceAsync(invoice, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveLineItemAsync(RemoveLineItem command, CancellationToken cancellationToken = default)
    {
        var invoice = await GetInvoiceOrThrowAsync(command.InvoiceId, cancellationToken);

        var lineItem = invoice.LineItems.FirstOrDefault(li => li.Id == command.LineItemId);

        if (lineItem is null)
        {
            return Result.Failure("Line item not found.");
        }

        invoice.RemoveLineItem(lineItem);

        await InvoiceManager.UpdateInvoiceAsync(invoice, cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// Processes a payment for an invoice
    /// </summary>
    public async Task<Result> ProcessPaymentAsync(ProcessPayment command, CancellationToken cancellationToken = default)
    {
        var invoice = await GetInvoiceOrThrowAsync(command.InvoiceId, cancellationToken);

        var paymentMethod = await PaymentMethodRepository.GetAsync(command.PaymentMethodId, cancellationToken: cancellationToken)
            ?? throw new ArgumentException($"Payment method {command.PaymentMethodId} not found");

        var payment = new Payment(command.Amount, DateOnly.FromDateTime(DateTime.UtcNow), paymentMethod, command.ReferenceNumber)
        {
            Notes = command.Notes,
            GatewayTransactionId = command.GatewayTransactionId ?? String.Empty,
            GatewayName = command.GatewayName ?? String.Empty
        };

        invoice.ProcessPayment(payment);

        await InvoiceManager.UpdateInvoiceAsync(invoice, cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// Processes a refund for an invoice
    /// </summary>
    public async Task<Result> ProcessRefundAsync(ProcessRefund command, CancellationToken cancellationToken = default)
    {
        var invoice = await GetInvoiceOrThrowAsync(command.InvoiceId, cancellationToken);

        var refund = new Refund(command.OriginalPaymentId, command.Amount, command.Reason);

        invoice.ProcessRefund(refund);

        await InvoiceManager.UpdateInvoiceAsync(invoice, cancellationToken);

        return Result.Success();
    }

    private async Task<Invoice> GetInvoiceOrThrowAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await InvoiceManager.GetInvoiceAsync(invoiceId, cancellationToken)
            ?? throw new ArgumentException($"Invoice {invoiceId} not found");
    }

    private async Task<IEnumerable<LineItem>> CreateLineItemsAsync(DateOnly invoiceDate, IEnumerable<CreateLineItem> items, CancellationToken cancellationToken = default)
    {
        var lineItems = new List<LineItem>();

        foreach (var itemRequest in items)
        {
            var item = await CreateLineItemAsync(invoiceDate, itemRequest.ItemId, itemRequest.Quantity, cancellationToken)
                ?? throw new ArgumentException($"Item {itemRequest.ItemId} not found");

            lineItems.Add(item);
        }

        return lineItems;
    }

    private async Task<LineItem> CreateLineItemAsync(DateOnly invoiceDate, Guid itemId, Decimal quantity, CancellationToken cancellationToken = default)
    {
        var item = await ItemRepository.GetAsync(itemId, cancellationToken: cancellationToken)
            ?? throw new ArgumentException("Invalid item ID.");

        var lineItem = await LineItemFactory.CreateAsync(item, invoiceDate, cancellationToken);

        lineItem.ChangeQuantity(quantity);

        return lineItem;
    }
}

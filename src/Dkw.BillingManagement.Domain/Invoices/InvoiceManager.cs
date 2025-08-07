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
using Dkw.BillingManagement.Provinces;
using Dkw.BillingManagement.Taxes;
using Volo.Abp.DependencyInjection;

namespace Dkw.BillingManagement.Invoices;

[ExposeServices(typeof(IInvoiceManager))]
public class InvoiceManager(
    IInvoiceRepository invoiceRepository,
    ILineItemFactory lineItemFactory,
    IItemRepository itemRepository,
    ITaxCodeRepository taxCodeRepository,
    ITaxManager taxProvider,
    IProvinceRepository provinceManager,
    TimeProvider timeProvider)
    : BillingManagementDomainService, IInvoiceManager, ITransientDependency
{
    public IInvoiceRepository InvoiceRepository { get; } = invoiceRepository;
    public ILineItemFactory LineItemFactory { get; } = lineItemFactory;
    public IItemRepository ItemRepository { get; } = itemRepository;
    public ITaxCodeRepository TaxCodeRepository { get; } = taxCodeRepository;
    public ITaxManager TaxProvider { get; } = taxProvider;
    public IProvinceRepository ProvinceRepository { get; } = provinceManager;
    public TimeProvider TimeProvider { get; } = timeProvider;

    public async Task<Guid> CreateInvoiceAsync(Customer customer, IEnumerable<LineItem> lineItems, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(lineItems);

        var placeOfSupply = await ProvinceRepository.GetAsync(customer.BillingAddress.Province, cancellationToken: cancellationToken);

        var invoice = new Invoice(GuidGenerator.Create(), customer, placeOfSupply, Today, lineItems);

        var inserted = await InvoiceRepository.InsertAsync(invoice, autoSave: true, cancellationToken: cancellationToken);

        return inserted.Id;
    }

    public async Task<Invoice> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await InvoiceRepository.GetAsync(invoiceId, cancellationToken: cancellationToken)
            ?? throw new BillingManagementException(ErrorCodes.NotFound, $"Invoice with ID {invoiceId} not found.");

        return invoice;
    }

    public Task<Invoice> UpdateInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task ApplyRefundAsync(Invoice invoice, Refund refund, CancellationToken cancellationToken = default)
    {
        var invoiceTotal = invoice.GetGrandTotal();
        if (refund.Amount > invoiceTotal)
        {
            throw new InvalidOperationException($"Refund amount ({refund.Amount:c}) cannot exceed invoice total ({invoiceTotal:c}).");
        }

        var proportion = refund.Amount / invoiceTotal;

        //var subtotalRefund = Math.Round(invoice.GetTotalWithDiscountsAndShipping() * proportion, 2);
        var subtotalRefund = Math.Round((invoice.GetSubtotal() - invoice.GetDiscountAmount() + invoice.GetShippingAmount()) * proportion, 2);
        var taxRefund = Math.Round(invoice.GetTaxAmount() * proportion, 2);
        var shippingRefund = refund.IsShippingRefunded ? Math.Round(invoice.Shipping.ShippingCost * proportion, 2) : Decimal.Zero;

        refund.SetAmounts(
            subtotalRefund,
            taxRefund,
            shippingRefund
        );

        invoice.ProcessRefund(refund);

        await InvoiceRepository.UpdateAsync(invoice, autoSave: true, cancellationToken: cancellationToken);
    }

    public async Task AddLineItem(Invoice invoice, LineItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(item);

        invoice.AddLineItem(item);

        await InvoiceRepository.UpdateAsync(invoice, autoSave: true, cancellationToken: cancellationToken);
    }

    public async Task RemoveLineItemAsync(Invoice invoice, LineItem lineItem, CancellationToken cancellationToken = default)
    {

        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(lineItem);

        invoice.RemoveLineItem(lineItem);
        await InvoiceRepository.UpdateAsync(invoice, autoSave: true, cancellationToken);
    }

    public async Task ApplyTaxesAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        var rates = await TaxProvider.GetTaxesAsync(invoice.PlaceOfSupply, invoice.InvoiceDate, cancellationToken);

        invoice.ApplyTaxes(rates);

        await InvoiceRepository.UpdateAsync(invoice, autoSave: true, cancellationToken: cancellationToken);
    }

    public async Task<LineItem> CreateLineItemAsync(Guid taxableProductId, DateOnly invoiceDate, CancellationToken cancellationToken = default)
    {
        var item = await ItemRepository.GetAsync(taxableProductId, cancellationToken: cancellationToken)
            ?? throw new BillingManagementException(ErrorCodes.NotFound, $"Item with ID {taxableProductId} not found.");

        return await LineItemFactory.CreateAsync(item, invoiceDate, cancellationToken);
    }
}

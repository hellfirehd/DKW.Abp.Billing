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

using Dkw.BillingManagement.Items;
using Dkw.BillingManagement.Provinces;
using Dkw.BillingManagement.Taxes;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace Dkw.BillingManagement.Invoices;

[ExposeServices(typeof(IInvoiceManager))]
public class InvoiceManager(
    IInvoiceRepository invoiceRepository,
    IItemRepository itemRepository,
    ITaxCodeRepository taxCodeRepository,
    ITaxProvider taxProvider,
    IProvinceRepository provinceManager)
    : DomainService, IInvoiceManager, ITransientDependency
{
    public IInvoiceRepository InvoiceRepository { get; } = invoiceRepository;
    public IItemRepository ItemRepository { get; } = itemRepository;
    public ITaxCodeRepository TaxCodeRepository { get; } = taxCodeRepository;
    public ITaxProvider TaxProvider { get; } = taxProvider;
    public IProvinceRepository ProvinceRepository { get; } = provinceManager;
    public Task<Guid> CreateInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<Invoice> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <summary>
    /// Gets applicable taxes for an item based on its tax code and customer profile
    /// </summary>
    public async Task<IEnumerable<ApplicableTax>> GetApplicableTaxesAsync(LineItem item, CustomerTaxProfile customerProfile, DateOnly effectiveDate)
    {
        // Check customer exemption first
        if (customerProfile.QualifiesForExemption(effectiveDate) == true)
        {
            return [];
        }

        // Get item classification
        if ((item.TaxClasification is null || !item.TaxClasification.IsValidOn(effectiveDate)) && String.IsNullOrEmpty(item.ItemInfo.TaxCode))
        {
            // Fallback to standard taxes if no classification
            return await TaxProvider.GetApplicableTaxesAsync(customerProfile.TaxProvince, effectiveDate);
        }

        var code = item.TaxClasification?.TaxCode ?? item.ItemInfo.TaxCode;

        // Get tax code
        var taxCode = await TaxCodeRepository.GetAsync(code);
        if (taxCode is null || !taxCode.IsValidOn(effectiveDate))
        {
            // Fallback to standard taxes if tax code not found
            return await TaxProvider.GetApplicableTaxesAsync(customerProfile.TaxProvince, effectiveDate);
        }

        // Apply tax treatment rules
        return taxCode.TaxTreatment switch
        {
            TaxTreatment.Exempt or TaxTreatment.OutOfScope => [],
            TaxTreatment.ZeroRated => await GetZeroRatedTaxRatesAsync(customerProfile.TaxProvince, effectiveDate),
            _ => await TaxProvider.GetApplicableTaxesAsync(customerProfile.TaxProvince, effectiveDate)
        };
    }

    private async Task<List<ApplicableTax>> GetZeroRatedTaxRatesAsync(Province taxProvince, DateOnly effectiveDate)
    {
        // Zero-rated items typically have a specific tax rate of 0%
        var list = await TaxProvider.GetApplicableTaxesAsync(taxProvince, effectiveDate);

        return list
            .Select(at => at with { Rate = 0.0m })
            .ToList();
    }

    /// <summary>
    /// Calculates the proper breakdown based on invoice totals and applies the refund to the invoice.
    /// </summary>
    public async Task ApplyRefundAsync(Invoice invoice, Refund refund, CancellationToken cancellationToken)
    {
        var invoiceTotal = invoice.GetTotal();
        if (refund.Amount > invoiceTotal)
        {
            throw new InvalidOperationException($"Refund amount ({refund.Amount:c}) cannot exceed invoice total ({invoiceTotal:c}).");
        }

        var proportion = refund.Amount / invoiceTotal;

        var subtotalRefund = Math.Round(invoice.GetTotalWithDiscountsAndShipping() * proportion, 2);
        var taxRefund = Math.Round(invoice.GetTotalTax() * proportion, 2);
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

    public async Task RemoveLineItemAsync(Invoice invoice, LineItem lineItem, CancellationToken cancellationToken)
    {

        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(lineItem);

        invoice.RemoveLineItem(lineItem);
        await InvoiceRepository.UpdateAsync(invoice, autoSave: true, cancellationToken);
    }
}

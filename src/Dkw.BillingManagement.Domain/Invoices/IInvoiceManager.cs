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

using Dkw.BillingManagement.Taxes;
using Volo.Abp.Domain.Services;

namespace Dkw.BillingManagement.Invoices;
public interface IInvoiceManager : IDomainService
{
    Task AddLineItem(Invoice invoice, LineItem item, CancellationToken cancellationToken = default);
    Task ApplyRefundAsync(Invoice invoice, Refund refund, CancellationToken cancellationToken);
    Task<Guid> CreateInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task<Invoice> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicableTax>> GetApplicableTaxesAsync(LineItem item, CustomerTaxProfile customerProfile, DateOnly effectiveDate);
    Task RemoveLineItemAsync(Invoice invoice, LineItem lineItem, CancellationToken cancellationToken);
}

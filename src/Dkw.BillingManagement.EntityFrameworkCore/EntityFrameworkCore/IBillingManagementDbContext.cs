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
using Dkw.BillingManagement.Invoices;
using Dkw.BillingManagement.Invoices.LineItems;
using Dkw.BillingManagement.Items;
using Dkw.BillingManagement.Payments;
using Dkw.BillingManagement.Provinces;
using Dkw.BillingManagement.Taxes;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Dkw.BillingManagement.EntityFrameworkCore;

[ConnectionStringName(BillingManagementDbProperties.ConnectionStringName)]
public interface IBillingManagementDbContext : IEfCoreDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Discount> Discounts { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<LineItem> LineItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PaymentMethod> PaymentMethods { get; }
    DbSet<ItemBase> ItemBase { get; }
    DbSet<ItemInfo> ItemInfo { get; }
    DbSet<Province> Provinces { get; }
    DbSet<Refund> Refunds { get; }
    DbSet<Tax> Taxes { get; }
    DbSet<TaxCode> TaxCodes { get; }
}

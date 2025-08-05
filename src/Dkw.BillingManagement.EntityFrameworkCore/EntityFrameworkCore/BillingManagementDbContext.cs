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
using Dkw.BillingManagement.Items;
using Dkw.BillingManagement.Payments;
using Dkw.BillingManagement.Provinces;
using Dkw.BillingManagement.Shipping;
using Dkw.BillingManagement.Taxes;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Dkw.BillingManagement.EntityFrameworkCore;

[ConnectionStringName(BillingManagementDbProperties.ConnectionStringName)]
public class BillingManagementDbContext(DbContextOptions<BillingManagementDbContext> options)
    : AbpDbContext<BillingManagementDbContext>(options), IBillingManagementDbContext
{
    public DbSet<Customer> Customers { get; set; } = default!;
    public DbSet<Discount> Discounts { get; set; } = default!;
    public DbSet<Invoice> Invoices { get; set; } = default!;
    public DbSet<LineItem> LineItems { get; set; } = default!;
    public DbSet<Payment> Payments { get; set; } = default!;
    public DbSet<PaymentMethod> PaymentMethods { get; set; } = default!;
    public DbSet<ItemBase> ItemBase { get; set; } = default!;
    public DbSet<Province> Provinces { get; set; } = default!;
    public DbSet<Refund> Refunds { get; set; } = default!;
    public DbSet<Tax> Taxes { get; set; } = default!;
    public DbSet<TaxCode> TaxCodes { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
#endif
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Address>().HaveConversion<AddressConverter>();
        configurationBuilder.Properties<DateOnly>().HaveConversion<DateOnlyConverter>();
        configurationBuilder.Properties<Email>().HaveConversion<EmailConverter>();
        configurationBuilder.Properties<ItemInfo>().HaveConversion<ItemInfoValueConverter>();
        configurationBuilder.Properties<ShippingInfo>().HaveConversion<ShippingInfoConverter>();
        configurationBuilder.Properties<AppliedTax>().HaveConversion<AppliedTaxValueConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureBillingManagement();
    }
}

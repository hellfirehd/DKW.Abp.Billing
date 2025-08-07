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
using Volo.Abp.Domain.Entities;

namespace Dkw.BillingManagement.Invoices.LineItems;

/// <summary>
/// Base class for all invoice items (products and services)
/// </summary>
public class LineItem(Guid id) : Entity<Guid>(id)
{
    private readonly List<AppliedTax> _appliedTaxes = [];
    private readonly List<Discount> _appliedDiscounts = [];

    public virtual Invoice Invoice { get; set; } = default!;
    public virtual Guid InvoiceId { get; set; }

    public virtual ItemInfo ItemInfo { get; set; } = default!;

    public virtual TaxClassification TaxClasification { get; set; } = default!;
    public virtual DateOnly InvoiceDate { get; set; }
    public virtual Decimal Quantity { get; set; } = 1.0m;

    public virtual Int32 SortOrder { get; set; }

    public virtual IReadOnlyList<AppliedTax> AppliedTaxes => _appliedTaxes;
    public virtual IReadOnlyList<Discount> AppliedDiscounts => _appliedDiscounts;

    public virtual Decimal UnitPrice => ItemInfo.UnitPrice;

    public LineItem ChangeQuantity(Decimal newQuantity)
    {
        if (newQuantity < Decimal.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity must be greater than zero.");
        }

        Quantity = newQuantity;

        return this;
    }

    /// <summary>
    /// Applies taxes to this line item
    /// </summary>
    public void ApplyTaxes(IEnumerable<ApplicableTax> taxRates)
    {
        _appliedTaxes.Clear();
        var taxableAmount = GetSubtotal() - GetDiscountTotal();

        foreach (var taxRate in taxRates)
        {
            // How do we determine if a tax rate applies?
            var taxAmount = new AppliedTax
            {
                Name = taxRate.Name,
                Rate = taxRate.Rate,
                TaxableAmount = taxableAmount
            };
            _appliedTaxes.Add(taxAmount);
        }
    }

    /// <summary>
    /// Applies discounts to this line item
    /// </summary>
    public void ApplyDiscounts(IEnumerable<Discount> discounts)
    {
        ArgumentNullException.ThrowIfNull(discounts);

        _appliedDiscounts.Clear();
        foreach (var discount in discounts.Where(d => d.Scope == DiscountScope.PerItem && d.IsActive))
        {
            _appliedDiscounts.Add(discount);
        }
    }

    /// <summary>
    /// Gets the subtotal before taxes and discounts
    /// </summary>
    public virtual Decimal GetSubtotal() => ItemInfo.UnitPrice * Quantity;

    /// <summary>
    /// Gets the total discount amount for this line item
    /// </summary>
    public virtual Decimal GetDiscountTotal()
    {
        if (AppliedDiscounts.Count == 0)
        {
            return 0.00m;
        }

        var subtotal = GetSubtotal();

        foreach (var discount in AppliedDiscounts)
        {
            subtotal -= discount.GetAmount(subtotal);
        }

        // Ensure subtotal does not go below zero
        return Math.Max(0, subtotal);
    }

    /// <summary>
    /// Gets the total tax amount for this line item
    /// </summary>
    public virtual Decimal GetTaxTotal() => AppliedTaxes.Sum(t => t.Tax);

    /// <summary>
    /// Gets the final total including taxes
    /// </summary>
    public virtual Decimal GetTotal() => GetSubtotal() - GetDiscountTotal() + GetTaxTotal();
}

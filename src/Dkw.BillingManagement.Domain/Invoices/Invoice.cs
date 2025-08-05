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
using Dkw.BillingManagement.Payments;
using Dkw.BillingManagement.Provinces;
using Dkw.BillingManagement.Shipping;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dkw.BillingManagement.Invoices;

/// <summary>
/// Core invoice aggregate root
/// </summary>
public class Invoice : FullAuditedAggregateRoot<Guid>
{
    private readonly List<LineItem> _lineItems = [];
    private readonly List<Discount> _discounts = [];
    private readonly List<Surcharge> _surcharges = [];
    private readonly List<ApplicableTax> _applicableTaxes = [];
    private readonly List<Payment> _payments = [];
    private readonly List<Refund> _refunds = [];
    private Address billingAddress = Address.Empty;
    private Address shippingAddress = Address.Empty;

    protected Invoice()
    {
        // Parameterless constructor for EF Core/JSON serialization
    }

    public Invoice(
        Guid invoiceId,
        Customer customer,
        Guid placeOfSupplyId,
        DateOnly invoiceDate,
        IEnumerable<LineItem>? items = null,
        IEnumerable<Discount>? discounts = null,
        IEnumerable<Surcharge>? surcharges = null) : base(invoiceId)
    {
        InvoiceDate = invoiceDate;  // ToDo: Validate that the date is not insane.

        Customer = customer ?? throw new ArgumentNullException(nameof(customer));

        PlaceOfSupplyId = placeOfSupplyId;

        if (items is not null)
        {
            _lineItems.AddRange(items.Where(i => i != null));
        }

        if (discounts is not null)
        {
            _discounts.AddRange(discounts.Where(d => d != null));
        }

        if (surcharges is not null)
        {
            _surcharges.AddRange(surcharges.Where(s => s != null));
        }

        Recalculate();
    }

    // Properties

    public virtual String InvoiceNumber { get; protected set; } = String.Empty;
    public virtual DateOnly InvoiceDate { get; protected set; }
    public virtual DateOnly DueDate { get; set; }
    public virtual InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public virtual String ReferenceNumber { get; protected set; } = String.Empty;
    public virtual ShippingInfo Shipping { get; protected set; } = ShippingInfo.Empty;

    // Relationships
    public virtual Guid CustomerId { get; protected set; }
    public virtual Customer Customer { get; protected set; } = default!;

    public virtual Guid PlaceOfSupplyId { get; protected set; }
    public virtual Province PlaceOfSupply { get; protected set; } = default!;

    public virtual Address BillingAddress
    {
        get => billingAddress ?? Customer.BillingAddress ?? Address.Empty;
        protected set => billingAddress = value;
    }

    public virtual Address ShippingAddress
    {
        get => shippingAddress ?? Customer.ShippingAddress ?? Address.Empty;
        protected set => shippingAddress = value;
    }

    // Line Items
    public virtual IReadOnlyList<LineItem> LineItems => _lineItems;

    // Order Level Discounts
    public virtual IReadOnlyList<Discount> Discounts => _discounts;

    // Surcharges
    public virtual IReadOnlyList<Surcharge> Surcharges => _surcharges;

    public virtual IReadOnlyList<ApplicableTax> ApplicableTaxes => _applicableTaxes;

    public virtual IReadOnlyList<Payment> Payments => _payments;

    public virtual IReadOnlyList<Refund> Refunds => _refunds;

    // Methods that modify the invoice

    /// <summary>
    /// Sets the tax rates and recalculates taxes
    /// </summary>
    public void ApplyTaxes(IEnumerable<ApplicableTax> taxes)
    {
        _applicableTaxes.Clear();
        _applicableTaxes.AddRange(taxes.Where(t => t != null));

        // Apply taxes to each line item
        foreach (var item in LineItems)
        {
            item.ApplyTaxes(_applicableTaxes);
        }

        Recalculate();
    }

    public void SetShipping(ShippingInfo shipping)
    {
        Shipping = shipping ?? throw new ArgumentNullException(nameof(shipping));

        Recalculate();
    }

    /// <summary>
    /// Adds an item to the invoice
    /// </summary>
    public void AddLineItem(LineItem item)
    {
        _lineItems.Add(item);

        Recalculate();
    }

    public void RemoveLineItem(LineItem lineItem)
    {
        _lineItems.Remove(lineItem);

        Recalculate();
    }

    /// <summary>
    /// Removes an item from the invoice
    /// </summary>
    public void RemoveLineItem(Guid itemId)
    {
        _lineItems.RemoveAll(i => i.Id == itemId);

        Recalculate();
    }

    /// <summary>
    /// Adds an order-level discount
    /// </summary>
    public void AddOrderDiscount(Discount discount)
    {
        if (discount.Scope == DiscountScope.PerOrder)
        {
            _discounts.Add(discount);

            Recalculate();
        }
        else
        {
            throw new BillingManagementException(ErrorCodes.InvalidDiscountScope, "Only order-level discounts can be applied to the invoice.");
        }
    }

    /// <summary>
    /// Adds a surcharge to the invoice
    /// </summary>
    public void AddSurcharge(Surcharge surcharge)
    {
        _surcharges.Add(surcharge);

        Recalculate();
    }

    /// <summary>
    /// Adds a payment to the invoice
    /// </summary>
    public void ProcessPayment(Payment payment)
    {
        _payments.Add(payment);
        UpdateStatus();
    }

    /// <summary>
    /// Processes a refund
    /// </summary>
    public void ProcessRefund(Refund refund)
    {
        var balance = GetTotalPaid() - GetTotalRefunded();
        if (refund.Amount > balance)
        {
            throw new BillingManagementException(ErrorCodes.InvalidRefundAmount, $"Refund amount ({refund.Amount:c}) exceeds net payments received: {balance:c}");
        }

        _refunds.Add(refund);
        UpdateStatus();
    }

    /// <summary>
    /// Marks the invoice as sent
    /// </summary>
    public void PostInvoice()
    {
        if (Status is InvoiceStatus.Draft or InvoiceStatus.Pending)
        {
            Status = InvoiceStatus.Posted;
        }
    }

    /// <summary>
    /// Cancels the invoice
    /// </summary>
    public void CancelInvoice()
    {
        if (Status != InvoiceStatus.Paid && GetTotalPaid() == 0)
        {
            Status = InvoiceStatus.Cancelled;
        }
        else
        {
            throw new BillingManagementException(ErrorCodes.CannotCancelInvoice, $"This invoice has payments applied: {GetTotalPaid():c}");
        }
    }

    /// <summary>
    /// Updates the invoice status based on payments and balance
    /// </summary>
    public void UpdateStatus()
    {
        var balance = GetBalance();
        var totalRefunded = GetTotalRefunded();
        var total = GetTotal();

        if (totalRefunded >= total)
        {
            Status = InvoiceStatus.Refunded;
        }
        else if (totalRefunded > 0)
        {
            Status = InvoiceStatus.PartiallyRefunded;
        }
        else if (balance <= 0)
        {
            Status = InvoiceStatus.Paid;
        }
        else if (DateOnly.FromDateTime(DateTime.UtcNow) > DueDate)
        {
            Status = InvoiceStatus.Overdue;
        }
        else if (Status == InvoiceStatus.Draft)
        {
            Status = InvoiceStatus.Pending;
        }
    }

    protected void Recalculate()
    {
        // Recalculate taxes for all items
        foreach (var item in _lineItems)
        {
            item.ApplyTaxes(ApplicableTaxes);
        }
    }

    // Methods to calculate amounts

    /// <summary>
    /// Gets the subtotal of all line items before discounts and taxes
    /// </summary>
    public Decimal GetSubtotal()
        => _lineItems.Sum(item => item.GetSubtotal());

    /// <summary>
    /// Gets the total discount amount from line items
    /// </summary>
    public Decimal GetLineItemDiscounts()
        => _lineItems.Sum(item => item.GetDiscountTotal());

    /// <summary>
    /// Gets the total after line item discounts but before order discounts
    /// </summary>
    public Decimal GetDiscountedSubtotal()
        => _lineItems.Sum(item => item.GetDiscountTotal());

    /// <summary>
    /// Gets the total order-level discount amount
    /// </summary>
    public Decimal GetOrderDiscounts()
    {
        var discountedSubtotal = GetDiscountedSubtotal();
        return _discounts
            .Where(d => d.IsActive)
            .Sum(d => d.GetDiscount(discountedSubtotal));
    }

    /// <summary>
    /// Gets the total after all discounts but before taxes and surcharges
    /// </summary>
    public Decimal GetTotalWithDiscountsAndShipping()
        => Math.Max(0, GetDiscountedSubtotal() - GetOrderDiscounts() + Shipping.ShippingCost);

    /// <summary>
    /// Gets the total tax amount
    /// </summary>
    public Decimal GetTotalTax()
        => _lineItems.Sum(item => item.GetTaxTotal());

    /// <summary>
    /// Gets the total surcharge amount
    /// </summary>
    public Decimal GetTotalSurcharges()
    {
        var baseAmount = GetTotalWithDiscountsAndShipping() + GetTotalTax();
        return _surcharges
            .Where(s => s.IsActive)
            .Sum(s => s.CalculateSurchargeAmount(baseAmount));
    }

    /// <summary>
    /// Gets the final total amount due
    /// </summary>
    public Decimal GetTotal()
        => GetTotalWithDiscountsAndShipping() + GetTotalTax() + GetTotalSurcharges();

    /// <summary>
    /// Gets the total amount paid
    /// </summary>
    public Decimal GetTotalPaid() => _payments
        .Where(p => p.Status == PaymentStatus.Completed)
        .Sum(p => p.Amount);

    /// <summary>
    /// Gets the total amount refunded
    /// </summary>
    public Decimal GetTotalRefunded()
        => _refunds.Sum(r => r.Amount);

    /// <summary>
    /// Gets the outstanding balance of the invoice
    /// </summary>
    public Decimal GetBalance() => GetTotal() - GetTotalPaid() + GetTotalRefunded();
}

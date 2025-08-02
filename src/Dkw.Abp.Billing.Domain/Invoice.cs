using Dkw.Abp.Billing.Customers;

namespace Dkw.Abp.Billing;

/// <summary>
/// Core invoice aggregate root
/// </summary>
public class Invoice
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public String InvoiceNumber { get; set; } = String.Empty;
    public DateOnly InvoiceDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? DueDate { get; set; }
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;

    // Customer Information
    public Customer Customer { get; set; } = new Customer();
    public String CustomerName { get; set; } = String.Empty;
    public Province Province { get; set; } = Province.Empty; // For tax calculations

    // Line Items
    public List<InvoiceItem> Items { get; private set; } = [];

    // Order Level Discounts
    public List<Discount> OrderDiscounts { get; private set; } = [];

    // Surcharges
    public List<Surcharge> Surcharges { get; private set; } = [];

    // Shipping
    public Decimal ShippingCost { get; set; }
    public String ShippingMethod { get; set; } = String.Empty;

    // Payments and Refunds
    public List<Payment> Payments { get; private set; } = [];
    public List<Refund> Refunds { get; private set; } = [];

    // Tax Rates applied
    public List<TaxRate> ApplicableTaxes { get; private set; } = [];

    /// <summary>
    /// Adds an item to the invoice
    /// </summary>
    public void AddItem(InvoiceItem item)
    {
        Items.Add(item);
        RecalculateTotals();
    }

    /// <summary>
    /// Removes an item from the invoice
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        Items.RemoveAll(i => i.Id == itemId);
        RecalculateTotals();
    }

    /// <summary>
    /// Adds an order-level discount
    /// </summary>
    public void AddOrderDiscount(Discount discount)
    {
        if (discount.Scope == DiscountScope.PerOrder)
        {
            OrderDiscounts.Add(discount);
            RecalculateTotals();
        }
    }

    /// <summary>
    /// Adds a surcharge to the invoice
    /// </summary>
    public void AddSurcharge(Surcharge surcharge)
    {
        Surcharges.Add(surcharge);
        RecalculateTotals();
    }

    /// <summary>
    /// Sets the tax rates and recalculates taxes
    /// </summary>
    public void SetTaxRates(IEnumerable<TaxRate> taxRates)
    {
        ApplicableTaxes.Clear();
        ApplicableTaxes.AddRange(taxRates);

        // Apply taxes to each line item
        foreach (var item in Items)
        {
            item.ApplyTaxes(ApplicableTaxes);
        }

        // ToDo: Is this redundant?
        RecalculateTotals();
    }

    /// <summary>
    /// Gets the subtotal of all line items before discounts and taxes
    /// </summary>
    public Decimal GetSubtotal()
        => Items.Sum(item => item.GetSubtotal());

    /// <summary>
    /// Gets the total discount amount from line items
    /// </summary>
    public Decimal GetLineItemDiscounts()
        => Items.Sum(item => item.GetDiscountTotal());

    /// <summary>
    /// Gets the total after line item discounts but before order discounts
    /// </summary>
    public Decimal GetDiscountedSubtotal()
        => Items.Sum(item => item.GetDiscountedTotal());

    /// <summary>
    /// Gets the total order-level discount amount
    /// </summary>
    public Decimal GetOrderDiscounts()
    {
        var discountedSubtotal = GetDiscountedSubtotal();
        return OrderDiscounts
            .Where(d => d.IsActive)
            .Sum(d => d.CalculateDiscountAmount(discountedSubtotal));
    }

    /// <summary>
    /// Gets the total after all discounts but before taxes and surcharges
    /// </summary>
    public Decimal GetTotalAfterDiscounts()
        => Math.Max(0, GetDiscountedSubtotal() - GetOrderDiscounts() + ShippingCost);

    /// <summary>
    /// Gets the total tax amount
    /// </summary>
    public Decimal GetTotalTax()
        => Items.Sum(item => item.GetTaxTotal());

    /// <summary>
    /// Gets the total surcharge amount
    /// </summary>
    public Decimal GetTotalSurcharges()
    {
        var baseAmount = GetTotalAfterDiscounts() + GetTotalTax();
        return Surcharges
            .Where(s => s.IsActive)
            .Sum(s => s.CalculateSurchargeAmount(baseAmount));
    }

    /// <summary>
    /// Gets the final total amount due
    /// </summary>
    public Decimal GetTotal()
        => GetTotalAfterDiscounts() + GetTotalTax() + GetTotalSurcharges();

    /// <summary>
    /// Gets the total amount paid
    /// </summary>
    public Decimal GetTotalPaid() => Payments
        .Where(p => p.Status == PaymentStatus.Completed)
        .Sum(p => p.Amount);

    /// <summary>
    /// Gets the total amount refunded
    /// </summary>
    public Decimal GetTotalRefunded()
        => Refunds.Sum(r => r.Amount);

    /// <summary>
    /// Gets the outstanding balance
    /// </summary>
    public Decimal GetBalance() => GetTotal() - GetTotalPaid() + GetTotalRefunded();

    /// <summary>
    /// Adds a payment to the invoice
    /// </summary>
    public void AddPayment(Payment payment)
    {
        Payments.Add(payment);
        UpdateStatus();
    }

    /// <summary>
    /// Processes a refund
    /// </summary>
    public void ProcessRefund(Refund refund)
    {
        if (refund.Amount > GetTotalPaid() - GetTotalRefunded())
        {
            throw new InvalidOperationException("Refund amount cannot exceed net payments received");
        }

        Refunds.Add(refund);
        UpdateStatus();
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
        else if (DueDate.HasValue && DateOnly.FromDateTime(DateTime.UtcNow) > DueDate.Value)
        {
            Status = InvoiceStatus.Overdue;
        }
        else if (Status == InvoiceStatus.Draft)
        {
            Status = InvoiceStatus.Pending;
        }
    }

    /// <summary>
    /// Marks the invoice as sent
    /// </summary>
    public void MarkAsSent()
    {
        if (Status is InvoiceStatus.Draft or InvoiceStatus.Pending)
        {
            Status = InvoiceStatus.Sent;
        }
    }

    /// <summary>
    /// Cancels the invoice
    /// </summary>
    public void Cancel()
    {
        if (Status != InvoiceStatus.Paid && GetTotalPaid() == 0)
        {
            Status = InvoiceStatus.Cancelled;
        }
        else
        {
            throw new InvalidOperationException("Cannot cancel an invoice with payments");
        }
    }

    private void RecalculateTotals()
    {
        // Recalculate taxes for all items
        foreach (var item in Items)
        {
            item.ApplyTaxes(ApplicableTaxes);
        }
    }
}
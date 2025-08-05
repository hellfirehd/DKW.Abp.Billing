// DKW ABP Framework Extensions
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

namespace Dkw.BillingManagement.Payments;

public abstract class PaymentTerm
{
    /// <summary>
    ///  Should return the key of a localization resource that describes the payment term.
    /// </summary>
    public abstract String Name { get; }
}

public class DueOnReceiptPaymentTerm : PaymentTerm
{
    public override String Name => "Due on Receipt";
}

public class CashOnDeliveryPaymentTerm : PaymentTerm
{
    public override String Name => "Cash on Delivery";
}

public class CashInAdvancePaymentTerm : PaymentTerm
{
    public override String Name => "Cash in Advance";
}

public class PayInAdvancePaymentTerm : PaymentTerm
{
    public override String Name => "Pay in Advance";
}

public class EndOfMonthPaymentTerm : PaymentTerm
{
    public override String Name => "End of Month";
}

public class NetXPaymentTerm : PaymentTerm
{
    public override String Name => "Net X";
}

public class EarlyPaymentDiscountPaymentTerm : PaymentTerm
{
    public override String Name => "Early Payment Discount";
}

public class StagedPaymentsPaymentTerm : PaymentTerm
{
    public override String Name => "Staged Payments";
}

public class InstallmentsPaymentTerm : PaymentTerm
{
    public override String Name => "Installments";
}

public class ProjectCompletionPaymentTerm : PaymentTerm
{
    public override String Name => "Project Completion";
}

public class EndOfTermPaymentTerm : PaymentTerm
{
    public override String Name => "End of Term";
}

public enum PaymentTermType
{
    /// <summary>
    /// Represents a payment term indicating that payment is due immediately upon receipt of the invoice.
    /// </summary>
    /// <remarks>This payment term is typically used for transactions where immediate payment is required. It
    /// implies that the recipient should settle the invoice without delay.</remarks>
    DueOnReceipt,
    /// <summary>
    /// Represents a payment method where the customer pays for the goods or services upon delivery.
    /// </summary>
    /// <remarks>This payment method is commonly used in scenarios where customers prefer to inspect the goods
    /// before making a payment or do not have access to online payment options.</remarks>
    CashOnDelivery,

    /// <summary>
    /// Represents a payment method where the full amount is paid before goods or services are delivered.
    /// </summary>
    /// <remarks>This payment method is typically used in scenarios where the seller requires assurance of
    /// payment prior to fulfilling the order. It is commonly employed in high-risk transactions or when dealing with
    /// new customers.</remarks>
    CashInAdvance,

    /// <summary>
    /// Represents a payment method where the full amount is paid in advance before receiving goods or services.
    /// </summary>
    /// <remarks>This payment method is typically used in scenarios where upfront payment is required to
    /// secure a transaction. It ensures that the buyer commits to the purchase before the seller delivers the product
    /// or service.</remarks>
    PayInAdvance,

    /// <summary>
    /// Represents a payment term where payment is due at the end of the month in which the invoice is issued.
    /// </summary>
    /// <remarks>This payment term is commonly used in business transactions where the invoice is issued at the
    /// end of the month, allowing the customer to manage their cash flow accordingly.</remarks>
    EndOfMonth,

    /// <summary>
    /// Represents a payment term where payment is due at the end of the specified term.
    /// </summary>
    NetX,

    /// <summary>
    /// Represents a discount applied to a payment made before its due date.
    /// </summary>
    /// <remarks>This class encapsulates the details of an early payment discount, including the discount rate
    /// and the conditions under which it is applied. It is typically used in financial systems to incentivize timely
    /// payments.</remarks>
    EarlyPaymentDiscount,

    /// <summary>
    /// Represents a payment term where payments are made in stages based on project milestones.
    /// </summary>
    StagedPayments,

    /// <summary>
    /// Represents a payment term where payments are made in installments.
    /// </summary>
    Installments,

    /// <summary>
    /// Represents a payment term where payment is made at project completion.
    /// </summary>
    /// <remarks>This class can be used to track and manage the state of a project's completion, including
    /// whether it is finished, partially completed, or still in progress. It may also include additional metadata
    /// related to the project's completion.</remarks>
    ProjectCompletion,

    EndOfTerm,
}

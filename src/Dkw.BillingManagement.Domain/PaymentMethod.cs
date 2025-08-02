namespace Dkw.BillingManagement;

public abstract class PaymentMethod : IPaymentMethod
{
    public abstract PaymentType PaymentType { get; }
    public String Details { get; set; } = String.Empty; // e.g., card number, bank account number, etc.

    public static readonly IPaymentMethod BankTransfer = new BankTransferPaymentMethod();
    public static readonly IPaymentMethod CreditCard = new CreditCardPaymentMethod();
}

public class BankTransferPaymentMethod : PaymentMethod
{
    public override PaymentType PaymentType => PaymentType.BankTransfer;
}

public class CreditCardPaymentMethod : PaymentMethod
{
    public override PaymentType PaymentType => PaymentType.CreditCard;

    public String CardNumber { get; set; } = String.Empty;
    public String ExpiryDate { get; set; } = String.Empty; // Format: MM/YY
    public String CardHolderName { get; set; } = String.Empty;
    public String Cvv { get; set; } = String.Empty;
}
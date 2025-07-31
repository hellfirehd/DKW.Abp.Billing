namespace Billing;

public abstract class PaymentMethod : IPaymentMethod
{
    public abstract PaymentType PaymentType { get; }
    public string Details { get; set; } = String.Empty; // e.g., card number, bank account number, etc.

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

    public string CardNumber { get; set; } = string.Empty;
    public string ExpiryDate { get; set; } = string.Empty; // Format: MM/YY
    public string CardHolderName { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
}
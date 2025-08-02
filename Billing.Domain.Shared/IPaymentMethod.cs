namespace Billing;

public interface IPaymentMethod
{
    PaymentType PaymentType { get; }
    String Details { get; }
}
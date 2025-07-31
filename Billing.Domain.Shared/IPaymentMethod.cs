namespace Billing;

public interface IPaymentMethod
{
    PaymentType PaymentType { get; }
    string Details { get; }
}
namespace Dkw.BillingManagement;

public interface IPaymentMethod
{
    PaymentType PaymentType { get; }
    String Details { get; }
}
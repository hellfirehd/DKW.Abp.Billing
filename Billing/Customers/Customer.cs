namespace Billing.Customers;

public class Customer
{
    public Guid Id { get; set; }
    public String Name { get; set; } = String.Empty;
    public Email EmailAddresses { get; set; } = Email.Empty;
    public List<Address> Addresses { get; set; } = [];
    public List<PaymentMethod> PaymentMethods { get; set; } = [];
    public CustomerType CustomerType { get; set; } = CustomerType.Regular;
    public RecipientStatus RecipientStatus { get; set; } = RecipientStatus.Regular;
    public CustomerTaxProfile TaxProfile { get; set; }
}

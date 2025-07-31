namespace Billing;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<PhoneNumber> PhoneNumbers { get; set; } = [];
    public List<EmailAddress> EmailAddresses { get; set; } = [];
    public List<Address> Addresses { get; set; } = [];
    public List<PaymentMethod> PaymentMethods { get; set; } = [];

    public CustomerTaxProfile? TaxProfile { get; set; }
}

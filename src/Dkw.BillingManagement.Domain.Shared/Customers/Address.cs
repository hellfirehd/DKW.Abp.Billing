namespace Dkw.BillingManagement.Customers;

public class Address // Value Object
{
    public String Name { get; set; } = String.Empty;
    public String Line1 { get; set; } = String.Empty;
    public String Line2 { get; set; } = String.Empty;
    public String City { get; set; } = String.Empty;
    public Province Province { get; set; } = Province.Empty;
    public String PostalCode { get; set; } = String.Empty;
    public String Country { get; set; } = String.Empty;

    public PhoneNumber PhoneNumber { get; set; } = PhoneNumber.Empty;

    public Boolean IsDefault { get; set; }
    public Boolean IsShippingAddress { get; set; }
    public Boolean IsBillingAddress { get; set; }
}

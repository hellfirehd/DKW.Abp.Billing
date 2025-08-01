namespace Billing.Customers;

public class Address // Value Object
{
    public string Name { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string Line2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public Province Province { get; set; } = Province.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public PhoneNumber PhoneNumber { get; set; } = PhoneNumber.Empty;

    public bool IsDefault { get; set; }
    public bool IsShippingAddress { get; set; }
    public bool IsBillingAddress { get; set; }
}

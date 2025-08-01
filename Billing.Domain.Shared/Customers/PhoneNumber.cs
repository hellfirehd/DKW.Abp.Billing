namespace Billing.Customers;

public record PhoneNumber(String Number) // Value Object
{
    public static readonly PhoneNumber Empty = new(String.Empty);

    public override string ToString() => Number;
}

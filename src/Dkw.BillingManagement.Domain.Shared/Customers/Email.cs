namespace Dkw.BillingManagement.Customers;

public record Email(String EmailAddress) // Value Object
{
    public static readonly Email Empty = new(String.Empty);
    public String EmailAddress { get; init; } = EmailAddress;
    public override String ToString() => EmailAddress;

    public static implicit operator String(Email email) => email.EmailAddress;

    public static implicit operator Email(String email) => new(email);
}

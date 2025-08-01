namespace Billing.Customers;

public record Email(string EmailAddress) // Value Object
{
    public static readonly Email Empty = new(string.Empty);
    public string EmailAddress { get; init; } = EmailAddress;
    public override string ToString() => EmailAddress;

    public static implicit operator string(Email email) => email.EmailAddress;

    public static implicit operator Email(string email) => new(email);
}

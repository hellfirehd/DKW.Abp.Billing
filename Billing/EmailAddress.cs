namespace Billing;

public readonly struct EmailAddress(String email, String name)
{
    public String Email { get; } = email;
    public String Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
}

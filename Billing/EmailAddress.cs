namespace Billing;

public readonly struct EmailAddress(string email, string name)
{
    public string Email { get; } = email;
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
}

namespace Billing;

public readonly struct Email(string email)
{
    private readonly string _email = email ?? throw new ArgumentNullException(nameof(email));
}

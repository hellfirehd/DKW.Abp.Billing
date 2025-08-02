using System.Diagnostics;

namespace Dkw.Abp.Billing;

[DebuggerDisplay("{Code}")]
public sealed record Province : IEquatable<Province>, IComparable<Province>
{
    public static readonly Province Empty = new();

    public Province()
    {
        // Private constructor for Empty instance
    }

    internal Province(String code, String name, Boolean hasHst) : this()
    {
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        HasHST = hasHst;
    }

    public static Province Create(String code, String name, Boolean hasHst)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (code.Length != 2)
        {
            throw new ArgumentException("Province code must be exactly 2 characters long.", nameof(code));
        }

        return new(code, name, hasHst);
    }

    public String Code { get; } = String.Empty;
    public String Name { get; } = String.Empty;
    public Boolean HasHST { get; }

    public override String ToString() => Code;

    // IEquatable<Province> implementation
    public Boolean Equals(Province? other)
        => other is not null && String.Equals(Code, other.Code, StringComparison.OrdinalIgnoreCase);

    public override Int32 GetHashCode() => HashCode.Combine(Code.ToUpperInvariant(), Name.ToUpperInvariant());

    // IComparable<Province> implementation
    public Int32 CompareTo(Province? other)
    {
        if (other is null)
        {
            return 1;
        }

        // Compare by Name first, then by Code if Codes are equal
        var nameComparison = String.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        return nameComparison != 0 ? nameComparison : String.Compare(Code, other.Code, StringComparison.OrdinalIgnoreCase);
    }

    // Comparison operators
    public static Boolean operator <(Province left, Province right) => left.CompareTo(right) < 0;

    public static Boolean operator >(Province left, Province right) => left.CompareTo(right) > 0;

    public static Boolean operator <=(Province left, Province right) => left.CompareTo(right) <= 0;

    public static Boolean operator >=(Province left, Province right) => left.CompareTo(right) >= 0;

    public static implicit operator String(Province province) => province.ToString();
}


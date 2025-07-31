using System.Diagnostics;

namespace Billing;

[DebuggerDisplay("{Code}")]
public readonly struct Province : IEquatable<Province>, IComparable<Province>
{
    public static readonly Province Empty = new();

    public Province()
    {
        // Private constructor for Empty instance
    }

    internal Province(String code, String name) : this()
    {
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
    }

    public static Province Create(String code, String name) => new(code, name);

    public String Code { get; } = String.Empty;
    public String Name { get; } = String.Empty;

    public override String ToString() => Code;

    // IEquatable<Province> implementation
    public Boolean Equals(Province other)
        => String.Equals(Code, other.Code, StringComparison.OrdinalIgnoreCase);

    public override Boolean Equals(Object? obj) => obj is Province other && Equals(other);

    public override Int32 GetHashCode() => HashCode.Combine(Code.ToUpperInvariant(), Name.ToUpperInvariant());

    // IComparable<Province> implementation
    public Int32 CompareTo(Province other)
    {
        // Compare by Name first, then by Code if Codes are equal
        Int32 nameComparison = String.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        return nameComparison != 0 ? nameComparison : String.Compare(Code, other.Name, StringComparison.OrdinalIgnoreCase);
    }

    // Equality operators
    public static Boolean operator ==(Province left, Province right) => left.Equals(right);

    public static Boolean operator !=(Province left, Province right) => !(left == right);

    // Comparison operators
    public static Boolean operator <(Province left, Province right) => left.CompareTo(right) < 0;

    public static Boolean operator >(Province left, Province right) => left.CompareTo(right) > 0;

    public static Boolean operator <=(Province left, Province right) => left.CompareTo(right) <= 0;

    public static Boolean operator >=(Province left, Province right) => left.CompareTo(right) >= 0;

    public static implicit operator String(Province province) => province.ToString();
}

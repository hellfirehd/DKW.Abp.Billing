namespace Billing;

public readonly struct CorrelationId(Guid correlationId) : IEquatable<CorrelationId>, IComparable<CorrelationId>, IComparable<Guid>
{
    public static readonly CorrelationId Empty = new(Guid.Empty);

    private readonly Guid _id = correlationId != Guid.Empty ? correlationId : throw new ArgumentNullException(nameof(correlationId));

    public static implicit operator Guid(CorrelationId correlationId) => correlationId._id;

    public static implicit operator CorrelationId(Guid correlationId) => new(correlationId);

    public override String ToString() => _id.ToString();

    public override Boolean Equals(Object? obj) => obj is CorrelationId other && Equals(other);

    public Boolean Equals(CorrelationId other) => _id == other._id;

    public override Int32 GetHashCode() => _id.GetHashCode();

    public Int32 CompareTo(CorrelationId other) => throw new NotImplementedException();

    public Int32 CompareTo(Guid other) => throw new NotImplementedException();

    public static Boolean operator ==(CorrelationId left, CorrelationId right) => left.Equals(right);

    public static Boolean operator !=(CorrelationId left, CorrelationId right) => !left.Equals(right);

    public static Boolean operator <(CorrelationId left, CorrelationId right) => left.CompareTo(right) < 0;

    public static Boolean operator >(CorrelationId left, CorrelationId right) => left.CompareTo(right) > 0;

    public static Boolean operator <=(CorrelationId left, CorrelationId right) => left.CompareTo(right) <= 0;

    public static Boolean operator >=(CorrelationId left, CorrelationId right) => left.CompareTo(right) >= 0;
}

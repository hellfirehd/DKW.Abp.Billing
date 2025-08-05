// DKW Billing Management
// Copyright (C) 2025 Doug Wilson
//
// This program is free software: you can redistribute it and/or modify it under the terms of
// the GNU Affero General Public License as published by the Free Software Foundation, either
// version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License along with this
// program. If not, see <https://www.gnu.org/licenses/>.

using Dkw.BillingManagement.Taxes;
using System.Diagnostics;
using Volo.Abp.Domain.Entities;

namespace Dkw.BillingManagement.Provinces;

[DebuggerDisplay("{Code}:{Id}")]
public class Province : Entity<Guid>, IEquatable<Province>, IComparable<Province>
{
    public static readonly Province Empty = new();
    private readonly List<Tax> _taxes = [];

    protected Province()
    {
        // Private constructor for Empty instance
    }

    public Province(Guid id, String name, String code, Boolean hasHst, IEnumerable<Tax>? taxes = null) : base(id)
    {
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        HasHST = hasHst;

        if (taxes != null)
        {
            _taxes.AddRange(taxes);
        }
    }

    public static Province Create(Guid id, String code, String name, Boolean hasHst)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (code.Length != 2)
        {
            throw new ArgumentException("Province code must be exactly 2 characters long.", nameof(code));
        }

        return new(id, code, name, hasHst);
    }

    public virtual String Code { get; } = String.Empty;
    public virtual String Name { get; } = String.Empty;
    public virtual Boolean HasHST { get; }

    public virtual IReadOnlyList<Tax> Taxes => _taxes;

    public override String ToString() => Code;

    // IEquatable<Province> implementation
    public override Int32 GetHashCode() => HashCode.Combine(Code.ToUpperInvariant(), Name.ToUpperInvariant());

    public Boolean Equals(Province? other)
        => other is not null && String.Equals(Code, other.Code, StringComparison.OrdinalIgnoreCase);

    public override Boolean Equals(Object? obj)
    {
        if (obj is Province otherProvince)
        {
            return Equals(otherProvince);
        }

        return false;
    }

    public static Boolean operator ==(Province left, Province right)
    {
        if (left is Province leftProvince && right is Province rightProvince)
        {
            return leftProvince.Equals(rightProvince);
        }

        return false;
    }

    public static Boolean operator !=(Province left, Province right) => !(left == right);

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


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

namespace Dkw.BillingManagement.Shipping;

public record ShippingInfo : ValueObject<ShippingInfo>
{
    public static readonly ShippingInfo Empty = new EmptyShipping();
    private sealed record EmptyShipping : ShippingInfo
    {
        internal EmptyShipping()
        {
            ShippingCost = 0m;
            Carrier = String.Empty;
            TrackingNumber = String.Empty;
        }

        public override String Name => "No Shipping Selected";
    }

    public virtual String Name { get; protected set; } = String.Empty;
    public virtual String Carrier { get; protected set; } = String.Empty;
    public virtual String TrackingNumber { get; protected set; } = String.Empty;
    public virtual Decimal ShippingCost { get; protected set; }
    public virtual Boolean IsFreeShipping => ShippingCost <= 0m;
    public virtual Boolean IsRefundable { get; protected set; }
    public virtual void SetShippingCost(Decimal cost) => ShippingCost = cost;
}


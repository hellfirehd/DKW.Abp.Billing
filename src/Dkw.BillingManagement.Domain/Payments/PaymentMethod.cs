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

using Volo.Abp.Domain.Entities;

namespace Dkw.BillingManagement.Payments;

public abstract class PaymentMethod : Entity<Guid>
{

    public abstract String Discriminator { get; protected set; }   // e.g., "BankTransfer", "CreditCard", "Cash", etc.

    //public static readonly PaymentMethod BankTransfer = new BankTransferPaymentMethod();
    //public static readonly PaymentMethod CreditCard = new CreditCardPaymentMethod();
    //public static readonly PaymentMethod Cash = new CashPaymentMethod();
}

public class BankTransferPaymentMethod : PaymentMethod
{
    public override String Discriminator { get; protected set; } = "BankTransfer";
}

public class CreditCardPaymentMethod : PaymentMethod
{
    public override String Discriminator { get; protected set; } = "CreditCard";

    public String CardHolderName { get; set; } = String.Empty;
    public String CardNumber { get; set; } = String.Empty;
    public String ExpiryDate { get; set; } = String.Empty; // Format: MM/YY
    public String Cvv { get; set; } = String.Empty;
    public Address Address { get; set; } = Address.Empty;
}

public class CashPaymentMethod : PaymentMethod
{
    public override String Discriminator { get; protected set; } = "Cash"; // Adding Discriminator for Cash payment
}

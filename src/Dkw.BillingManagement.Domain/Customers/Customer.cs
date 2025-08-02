// DKW ABP Framework Extensions
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

namespace Dkw.BillingManagement.Customers;

public class Customer
{
    public Guid Id { get; set; }
    public String Name { get; set; } = String.Empty;
    public Email EmailAddresses { get; set; } = Email.Empty;
    public List<Address> Addresses { get; set; } = [];
    public List<PaymentMethod> PaymentMethods { get; set; } = [];
    public CustomerType CustomerType { get; set; } = CustomerType.Regular;
    public RecipientStatus RecipientStatus { get; set; } = RecipientStatus.Regular;
}

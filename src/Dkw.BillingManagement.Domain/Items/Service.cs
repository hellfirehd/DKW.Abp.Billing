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

namespace Dkw.BillingManagement.Items;

/// <summary>
/// Represents a service that can be sold
/// </summary>
public class Service : ItemBase<Service>
{
    public static Service Create(Guid id, String serviceType, String providerName, String taxCode, ItemCategory itemCategory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceType);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(taxCode);

        return new Service()
        {
            Id = id,
            ServiceType = serviceType,
            ProviderName = providerName,
            ItemCategory = itemCategory,
            TaxCode = taxCode
        };
    }

    public Service()
    {
        UnitType = "Hour";
    }

    public override ItemType ItemType { get; protected set; } = ItemType.Service;
    public String ServiceType { get; set; } = String.Empty;
    public String ProviderName { get; set; } = String.Empty;
    public DateOnly? ServiceDate { get; set; }
    public TimeSpan? Duration { get; set; }
}

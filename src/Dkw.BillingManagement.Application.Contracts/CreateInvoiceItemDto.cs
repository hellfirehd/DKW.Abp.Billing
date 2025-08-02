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

namespace Dkw.BillingManagement;

/// <summary>
/// DTO for creating invoice items
/// </summary>
public class CreateInvoiceItemDto
{
    public String Description { get; set; } = String.Empty;
    public Decimal UnitPrice { get; set; }
    public Int32 Quantity { get; set; } = 1;
    public String ItemType { get; set; } = "Product"; // "Product" or "Service"

    // Product specific fields
    public String? SKU { get; set; }
    public Decimal? Weight { get; set; }
    public String? Manufacturer { get; set; }
    public Boolean RequiresShipping { get; set; } = true;

    // Service specific fields
    public Decimal? HourlyRate { get; set; }
    public Decimal? Hours { get; set; }
}

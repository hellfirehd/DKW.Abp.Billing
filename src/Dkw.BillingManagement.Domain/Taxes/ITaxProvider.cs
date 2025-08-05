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

using Dkw.BillingManagement.Invoices;
using Dkw.BillingManagement.Provinces;
using Volo.Abp.Domain.Services;

namespace Dkw.BillingManagement.Taxes;

/// <summary>
/// Interface for tax calculation providers
/// </summary>
public interface ITaxProvider : IDomainService
{
    /// <summary>
    /// Gets all applicable tax rates for a province on a specific date
    /// </summary>
    Task<IEnumerable<ApplicableTax>> GetApplicableTaxesAsync(Province province, DateOnly? effectiveDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Default implementation does nothing. Implementors should probably override this to refresh tax data.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RefreshAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

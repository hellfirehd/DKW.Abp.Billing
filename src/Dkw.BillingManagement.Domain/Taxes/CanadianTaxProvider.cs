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
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace Dkw.BillingManagement.Taxes;

/// <summary>
/// Production-ready Canadian tax calculation provider
/// Supports all Canadian provinces and territories with accurate tax rates
/// </summary>
// ToDo: Refactor to load tax rates from a configuration file or database
[ExposeServices(typeof(ITaxProvider))]
public class CanadianTaxProvider(IProvinceRepository mapRepository, TimeProvider timeProvider)
    : DomainService, ITaxProvider, ITransientDependency
{
    private readonly IProvinceRepository _repository = mapRepository;

    protected TimeProvider TimeProvider { get; } = timeProvider;

    /// <summary>
    /// Gets applicable tax rates for a province and date
    /// </summary>
    public async Task<IEnumerable<ApplicableTax>> GetApplicableTaxesAsync(Province province, DateOnly? effectiveDate = null, CancellationToken cancellationToken = default)
    {
        var date = effectiveDate ?? DateOnly.FromDateTime(TimeProvider.GetUtcNow().DateTime);

        var p = await _repository.GetAsync(province.Id, includeDetails: true, cancellationToken: cancellationToken);

        var list = new List<ApplicableTax>();
        foreach (var entry in p.Taxes)
        {
            var rate = entry.GetTaxRate(date);
            if (rate is not null)
            {
                list.Add(new ApplicableTax(entry.Id, entry.Name, entry.Code, rate.Rate));
            }
        }

        return list;
    }

    public Task RefreshAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

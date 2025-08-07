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
using Dkw.BillingManagement.Invoices.LineItems;
using Dkw.BillingManagement.Provinces;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace Dkw.BillingManagement.Taxes;

/// <summary>
/// Production-ready Canadian tax calculation provider
/// Supports all Canadian provinces and territories with accurate tax rates
/// </summary>
[ExposeServices(typeof(ITaxManager))]
public class CanadianTaxManager(IProvinceRepository mapRepository, ITaxCodeRepository taxCodeRepository, TimeProvider timeProvider)
    : DomainService, ITaxManager, ITransientDependency
{
    private readonly IProvinceRepository _provinceRepository = mapRepository;
    private readonly ITaxCodeRepository _taxCodeRepository = taxCodeRepository;

    protected TimeProvider TimeProvider { get; } = timeProvider;

    /// <summary>
    /// Gets applicable tax rates for a province and date
    /// </summary>
    public async Task<IEnumerable<ApplicableTax>> GetTaxesAsync(Province province, DateOnly? effectiveDate = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(province);

        var date = effectiveDate ?? DateOnly.FromDateTime(TimeProvider.GetUtcNow().DateTime);

        var p = await _provinceRepository.GetAsync(province.Id, includeDetails: true, cancellationToken: cancellationToken);

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

    public async Task<IEnumerable<ApplicableTax>> GetTaxesAsync(LineItem item, CustomerTaxProfile customerProfile, DateOnly effectiveDate)
    {
        // Check customer exemption first
        if (customerProfile.QualifiesForExemption(effectiveDate) == true)
        {
            return [];
        }

        // Get item classification
        if ((item.TaxClasification is null || !item.TaxClasification.IsValidOn(effectiveDate)) && String.IsNullOrEmpty(item.ItemInfo.TaxCode))
        {
            // Fallback to standard taxes if no classification
            return await GetTaxesAsync(customerProfile.PlaceOfSupply, effectiveDate);
        }

        var code = item.TaxClasification?.TaxCode ?? item.ItemInfo.TaxCode;

        // Get tax code
        var taxCode = await _taxCodeRepository.GetAsync(code);
        if (taxCode is null || !taxCode.IsValidOn(effectiveDate))
        {
            // Fallback to standard taxes if tax code not found
            return await GetTaxesAsync(customerProfile.PlaceOfSupply, effectiveDate);
        }

        // Apply tax treatment rules
        return taxCode.TaxTreatment switch
        {
            TaxTreatment.Exempt or TaxTreatment.OutOfScope => [],
            TaxTreatment.ZeroRated => await GetZeroRatedTaxRatesAsync(customerProfile.PlaceOfSupply, effectiveDate),
            _ => await GetTaxesAsync(customerProfile.PlaceOfSupply, effectiveDate)
        };
    }

    private async Task<List<ApplicableTax>> GetZeroRatedTaxRatesAsync(Province taxProvince, DateOnly effectiveDate)
    {
        // Zero-rated items typically have a specific tax rate of 0%
        var list = await GetTaxesAsync(taxProvince, effectiveDate);

        return list
            .Select(at => at with { Rate = 0.0m })
            .ToList();
    }
}

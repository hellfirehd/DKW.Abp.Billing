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

using Dkw.BillingManagement.Provinces;
using Dkw.BillingManagement.Taxes;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;

namespace Dkw.BillingManagement;

public class BillingManagementDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IGuidGenerator GuidGenerator;
    private readonly IProvinceRepository _provinceRepository;

    private readonly Dictionary<String, Province> provinces;
    private readonly Dictionary<String, Tax> taxes;

    public BillingManagementDataSeedContributor(
        IGuidGenerator guidGenerator,
        IProvinceRepository provinceRepository,
        ITaxRepository taxRepository)
    {
        GuidGenerator = guidGenerator;
        _provinceRepository = provinceRepository;

        taxes = new Dictionary<String, Tax>
        {
            { "GST", new Tax(GuidGenerator.Create(), "GST", "GST")
                .AddTaxRate(0.05M, new DateOnly(2008, 1, 1), DateOnly.MaxValue)
                .AddTaxRate(0.06M, new DateOnly(2006, 7, 1), new DateOnly(2008, 1, 1))
                .AddTaxRate(0.07M, new DateOnly(1991, 1, 1), new DateOnly(2006, 7, 1)) },

            { "BC-PST", new Tax(GuidGenerator.Create(), "PST", "BC-PST")
                .AddTaxRate(0.07M, new DateOnly(2013, 4, 1), DateOnly.MaxValue) },

            { "MB-RST", new Tax(GuidGenerator.Create(), "RST", "MB-RST")
                .AddTaxRate(0.07M, new DateOnly(2019, 7, 1), DateOnly.MaxValue) },

            { "NB-HST", new Tax(GuidGenerator.Create(), "HST", "NB-HST")
                .AddTaxRate(0.15M, new DateOnly(2016, 7, 1), DateOnly.MaxValue) },

            { "NL-HST", new Tax(GuidGenerator.Create(), "HST", "NL-HST")
                .AddTaxRate(0.15M, new DateOnly(2016, 7, 1), DateOnly.MaxValue) },

            { "NS-HST", new Tax(GuidGenerator.Create(), "HST", "NS-HST")
                .AddTaxRate(0.15M, new DateOnly(2010, 7, 1), new DateOnly(2025, 4, 1))
                .AddTaxRate(0.14M, new DateOnly(2025, 4, 1), DateOnly.MaxValue) },

            { "ON-HST", new Tax(GuidGenerator.Create(), "HST", "ON-HST")
                .AddTaxRate(0.13M, new DateOnly(2010, 7, 1), DateOnly.MaxValue) },

            { "PE-HST", new Tax(GuidGenerator.Create(), "HST", "PE-HST")
                .AddTaxRate(0.15M, new DateOnly(2016, 10, 1), DateOnly.MaxValue) },

            { "QC-QST", new Tax(GuidGenerator.Create(), "QST", "QC-QST")
                .AddTaxRate(0.09975M, new DateOnly(2013, 1, 1), DateOnly.MaxValue) },

            { "SK-PST", new Tax(GuidGenerator.Create(), "PST", "SK-PST")
                .AddTaxRate(0.06M, new DateOnly(2017, 3, 23), DateOnly.MaxValue) }
        };

        provinces = new()
        {
            { "AB", new Province(GuidGenerator.Create(), "Alberta", "AB", hasHst: false, [taxes["GST"]]) },
            { "BC", new Province(GuidGenerator.Create(), "British Columbia", "BC", hasHst: false,[taxes["GST"],taxes["BC-PST"]]) },
            { "MB", new Province(GuidGenerator.Create(), "Manitoba", "MB", hasHst: false,[taxes["GST"],taxes["MB-RST"]]) },
            { "NB", new Province(GuidGenerator.Create(), "New Brunswick", "NB", hasHst: true, [taxes["NB-HST"]]) },
            { "NL", new Province(GuidGenerator.Create(), "Newfoundland and Labrador", "NL", hasHst: true, [taxes["NL-HST"]]) },
            { "NS", new Province(GuidGenerator.Create(), "Nova Scotia", "NS", hasHst: true, [taxes["NS-HST"]]) },
            { "NT", new Province(GuidGenerator.Create(), "Northwest Territories", "NT", hasHst: false, [taxes["GST"]]) },
            { "NU", new Province(GuidGenerator.Create(), "Nunavut", "NU", hasHst: false, [taxes["GST"]]) },
            { "ON", new Province(GuidGenerator.Create(), "Ontario", "ON", hasHst: true, [taxes["ON-HST"]]) },
            { "PE", new Province(GuidGenerator.Create(), "Prince Edward Island", "PE", hasHst: true, [taxes["PE-HST"]]) },
            { "QC", new Province(GuidGenerator.Create(), "Quebec", "QC", hasHst: false, [taxes["GST"], taxes["QC-QST"]]) },
            { "SK", new Province(GuidGenerator.Create(), "Saskatchewan", "SK", hasHst: false, [taxes["GST"], taxes["SK-PST"]]) },
            { "YT", new Province(GuidGenerator.Create(), "Yukon Territory", "YT", hasHst: false, [taxes["GST"]]) }
        };

        //provinceTaxMaps =
        //[
        //    new(provinces["AB"], taxes["GST"]),
        //    new(provinces["BC"], taxes["BC-PST"]),
        //    new(provinces["BC"], taxes["GST"]),
        //    new(provinces["MB"], taxes["GST"]),
        //    new(provinces["MB"], taxes["MB-RST"]),
        //    new(provinces["NB"], taxes["NB-HST"]),
        //    new(provinces["NL"], taxes["NL-HST"]),
        //    new(provinces["NS"], taxes["NS-HST"]),
        //    new(provinces["NT"], taxes["GST"]),
        //    new(provinces["NT"], taxes["GST"]),
        //    new(provinces["NU"], taxes["GST"]),
        //    new(provinces["ON"], taxes["ON-HST"]),
        //    new(provinces["PE"], taxes["PE-HST"]),
        //    new(provinces["QC"], taxes["GST"]),
        //    new(provinces["QC"], taxes["QC-QST"]),
        //    new(provinces["SK"], taxes["GST"]),
        //    new(provinces["SK"], taxes["GST"]),
        //    new(provinces["SK"], taxes["SK-PST"]),
        //    new(provinces["YT"], taxes["GST"])
        //];
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _provinceRepository.GetCountAsync() == 0)
        {
            await _provinceRepository.InsertManyAsync(provinces.Values, autoSave: true);
        }
    }
}

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

using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;

namespace Dkw.BillingManagement.Invoices.LineItems;

// Singleton so that we are not recreating the factories each time
[ExposeServices(typeof(ILineItemFactoryProvider))]
public class LineItemFactoryProvider : ILineItemFactoryProvider, ISingletonDependency
{
    private readonly Lazy<IEnumerable<IInvoiceLineItemFactory>> _lazyProviders;
    //protected LineItemFactoryOptions Options { get; }
    protected IServiceProvider ServiceProvider { get; }

    public IEnumerable<IInvoiceLineItemFactory> Factories => _lazyProviders.Value;

    public LineItemFactoryProvider(IServiceProvider serviceProvider)
    {
        //Options = options.Value;
        ServiceProvider = serviceProvider;

        _lazyProviders = new Lazy<IEnumerable<IInvoiceLineItemFactory>>(GetProviders, true);
    }

    protected virtual IEnumerable<IInvoiceLineItemFactory> GetProviders()
        => ServiceProvider.GetServices<IInvoiceLineItemFactory>();
}

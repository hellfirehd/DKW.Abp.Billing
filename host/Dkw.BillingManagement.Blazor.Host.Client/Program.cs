using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Dkw.BillingManagement.Blazor.Host.Client;

public class Program
{
    public async static Task Main(String[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        var application = await builder.AddApplicationAsync<DkwBillingManagementBlazorHostClientModule>(options =>
        {
            options.UseAutofac();
        });

        var host = builder.Build();

        await application.InitializeApplicationAsync(host.Services);

        await host.RunAsync();
    }
}

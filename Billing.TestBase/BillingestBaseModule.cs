//using Billing.TestBase;
//using Microsoft.Extensions.DependencyInjection;
//using Volo.Abp;
//using Volo.Abp.Autofac;
//using Volo.Abp.Modularity;

//namespace Billing;

//[DependsOn(typeof(AbpAutofacModule), typeof(AbpTestBaseModule))]
//public class BillingestBaseModule : AbpModule
//{
//    public override void OnApplicationInitialization(ApplicationInitializationContext context)
//    {
//        SeedTestData(context);
//    }

//    private static void SeedTestData(ApplicationInitializationContext context)
//    {
//        using (var scope = context.ServiceProvider.CreateScope())
//        {
//            var builder = scope.ServiceProvider.GetRequiredService<BillingTestDataBuilder>();

//        }
//    }
//}

using Volo.Abp.Modularity;

namespace Dkw.BillingManagement;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class BillingManagementApplicationTestBase<TStartupModule> : BillingManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}

using Volo.Abp.Modularity;

namespace Dkw.BillingManagement;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class BillingManagementDomainTestBase<TStartupModule> : BillingManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
